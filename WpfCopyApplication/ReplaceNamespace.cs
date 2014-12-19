using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using WpfCopyApplication.Model;


namespace WpfCopyApplication
{
    public class ReplaceNamespace
    {
        public static ObservableCollection<ListBoxItem> Log = new ObservableCollection<ListBoxItem>();
        public List<Conflict> ConflictList = new List<Conflict>();
 
        private DataReplacementRepository _repository;
        public ReplaceNamespace(ReplaceContext context)
        {
            _repository = new DataReplacementRepository(context);
        }

        public void ReplaceInFile(string sourceDir, string destDir, string oldNamespace, string newNamespace)
        {
//          file.CopyTo(sourceDir, true);
            FileInfo sourceFile = new FileInfo(sourceDir);
            FileInfo destFile = new FileInfo(destDir);
            var atr = sourceFile.Attributes;
//          destFile.Attributes &= ~FileAttributes.Hidden;
//            destFile.Attributes = FileAttributes.Archive;
            String strFile = File.ReadAllText(sourceDir);
            strFile = strFile.Replace(oldNamespace, newNamespace);
            File.WriteAllText(destDir, strFile);
//          destFile.Attributes |= FileAttributes.Hidden;
            destFile.Attributes = atr;
            
        }

        public void AddHistory(ReplaceRequest item)
        {
            _repository.AddHistory(item);        
        }

        public async Task DirectoryCopy(string sourceDirName, string destDirName, string newNamespace, string oldNamespace)
        {                    
            // Get the subdirectories for the specified directory.
            DirectoryInfo dir = new DirectoryInfo(sourceDirName);
            DirectoryInfo destDir = new DirectoryInfo(destDirName);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the destination directory doesn't exist, create it.
            if (!Directory.Exists(destDirName))
            {
                Directory.CreateDirectory(destDirName);
            }

            // Get the files in the directory and copy them to the new location.
            // FileInfo[] files = dir.GetFiles();

            var destFiles = destDir.GetFiles();
            List<FileInfo> files;

            //Check destination Folder 
            bool notEmptyDirectory = Directory.EnumerateFileSystemEntries(destDirName).Any();
            if (notEmptyDirectory)
            {
                files = await GetFilteredFiles(dir.GetFiles(), destFiles);
            }

            else files = dir.GetFiles().ToList();

            foreach (var file in files)
            {
                //  Situation 1: Folder empty 
                if (!notEmptyDirectory) Log.Add(new ListBoxItem() { Content = "File" + file.Name + " was added.", Background = Brushes.White });
                //  Replace name of file
                var replacedNameFile = file.Name.Replace(oldNamespace, newNamespace);
                if (file.Name.IndexOf(oldNamespace, System.StringComparison.Ordinal) != -1)
                {
                    Log.Add(new ListBoxItem() { Content = "File " + file.Name + " was renamed to " + replacedNameFile, Background = Brushes.Yellow });
                }
                
                //  Replace words inside file
                var tempPath = Path.Combine(destDirName, replacedNameFile);
                //  file.CopyTo(tempPath, true);
                ReplaceInFile(file.FullName, tempPath, oldNamespace, newNamespace);
                destFiles = destDir.GetFiles();
                _repository.AddDataReplace(file, tempPath, ComputeMD5Checksum(file.FullName), destFiles.FirstOrDefault(x => x.Name == replacedNameFile), ComputeMD5Checksum(tempPath));
            }
            
            // If copying subdirectories, copy them and their contents to new location.
            foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    await DirectoryCopy(subdir.FullName, temppath, newNamespace, oldNamespace);
                }
        }

        public async Task<List<FileInfo>> GetFilteredFiles(FileInfo[] files, FileInfo[] destFiles)
        {
            var filteredFiles = new List<FileInfo>();


            foreach (FileInfo file in files)
            {
                var destFile = destFiles.FirstOrDefault(x => x.Name == file.Name);

                if (_repository.TargetFileBySource(file.Name) != null)
                {
                    var targetPath = _repository.TargetFileBySource(file.Name);
                    destFile = destFiles.FirstOrDefault(x => x.Name == targetPath);
                }
                

                //   if (destFiles.FirstOrDefault(x => x.Name == file.Name) != null) conflictFiles.Add(new ConflictFiles() { FileFromSource = file, FileFromDest = destFiles.FirstOrDefault(x => x.Name == file.Name) });
                if (destFile == null)
                {
                    filteredFiles.Add(file);
                    Log.Add(new ListBoxItem() { Content = "File " + file.Name + " has been added.", Background = Brushes.White });
                }
                else if (FileUpdated(file, destFile))
                {
                    Log.Add(new ListBoxItem() { Content = "File " + file.Name + " was modified", Background = Brushes.Yellow });
                }
                else if (MergeFile(file, destFile))
                {
                    Log.Add(new ListBoxItem() { Content = "File " + file.Name + " need to merge", Background = Brushes.Red });
                    ConflictList.Add(new Conflict() { SourcePath = file.FullName, DestPath = destFiles.FirstOrDefault(x => x.Name == file.Name).FullName });
                }

            }

            foreach (FileInfo file in destFiles)
            {
                var sourceFile = files.FirstOrDefault(x => x.Name == file.Name);

                if (sourceFile == null)
                {
//                  Log.Add(new ListBoxItem() { Content = "File " + file.Name + " is not in the source folder", Background = Brushes.Yellow });
                    if (await NeedDelete(file))
                    {
                        file.Delete();
                    }

                }
            }

            return filteredFiles;
        }

        public bool IsBlankFolder(string destDirName)
        {
            DirectoryInfo dir = new DirectoryInfo(destDirName);

            if (!_repository.ConsistRecords(destDirName))
            {
                if (dir.Exists)
                {
                    if (Directory.EnumerateFileSystemEntries(destDirName).Any())
                    {
                        return true;
                    }
                }
                return false;
            }

            return false;

            // return !Directory.EnumerateFileSystemEntries(destDirName).Any();
        }
        private bool FileUpdated(FileInfo file, FileInfo destFile)
        {
            if(_repository.GetFileByPaths(file.FullName, destFile.FullName) == null) return false;
            if (_repository.IsDbEmpty())
            {
                var foundFile = _repository.GetFileByPaths(file.FullName, destFile.FullName);
                return !Compare(false, destFile, foundFile);
            }
            return true;

        }
        bool MergeFile(FileInfo file, FileInfo destFile)
        {
            if (_repository.IsDbEmpty() && _repository.GetFileByPaths(file.FullName, destFile.FullName) != null)
            {
                var foundFile = _repository.GetFileByPaths(file.FullName, destFile.FullName);
                return !Compare(true, file, foundFile);
            }
            return true;
        }

        public async Task<bool> NeedDelete(FileInfo file)
        {
            var FoundFile = await _repository.GetFileByTargetDirectory(file.FullName);

            if (FoundFile != null && _repository.IsExist(file.FullName) && FoundFile.DateTarget == file.LastWriteTime.Ticks && FoundFile.Size == file.Length)
            {
                if (FoundFile.Hash == ComputeMD5Checksum(file.FullName))
                {
                    _repository.RemoveByHash(FoundFile.Hash);
                    Log.Add(new ListBoxItem() { Content = "Warning! File " + file.FullName + " was deleted, because file already removed...", Background = Brushes.Red });
                   
                    return true;
                }
                return true;
            }
            return false;
        }



        private string ComputeMD5Checksum(string path)
        {
            using (FileStream fs = System.IO.File.OpenRead(path))
            {
                MD5 md5 = new MD5CryptoServiceProvider();
                byte[] fileData = new byte[fs.Length];
                fs.Read(fileData, 0, (int)fs.Length);
                byte[] checkSum = md5.ComputeHash(fileData);
                string result = BitConverter.ToString(checkSum).Replace("-", String.Empty);
                return result;
            }
        }

        /// <summary>
        /// Check file's data from db
        /// </summary>
        /// <param name="isSourceFile"> Cheking source/noSource file data in db</param>
        /// <param name="file">File from Source Folder</param>
        /// <param name="foundFile">Found file's data in bd</param>
        /// <returns></returns>
        bool Compare(bool isSourceFile, FileInfo file, DataReplacement foundFile)
        {

            if (isSourceFile)
            {
                if (file.LastWriteTime.Ticks == foundFile.Date && file.Length == foundFile.Size)
                {
                    return true;
                }
                else
                {
                    if (file.Length == foundFile.Size && ComputeMD5Checksum(file.FullName) == foundFile.Hash) return true;
                }

                return false;
            }
            else
            {
                if (file.LastWriteTime.Ticks == foundFile.DateTarget && file.Length == foundFile.SizeTarget)
                {
                    return true;
                }
                else
                {
                    if (file.Length == foundFile.SizeTarget && ComputeMD5Checksum(file.FullName) == foundFile.HashTarget) return true;
                }

                return false;
            }
        }
    }
}
