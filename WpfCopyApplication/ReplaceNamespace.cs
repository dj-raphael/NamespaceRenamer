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

        public void ReplaceInFile(string sourceDir, string oldNamespace, string newNamespace)
        {
            String strFile = File.ReadAllText(sourceDir);
            strFile = strFile.Replace(oldNamespace, newNamespace);
            File.WriteAllText(sourceDir, strFile);
        }

        public void AddHistory(ReplaceRequest item)
        {
            _repository.AddHistory(item);
            
        }

        public async Task DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, string newNamespace, string oldNamespace)
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
            bool isEmptyDirectory = Directory.EnumerateFileSystemEntries(destDirName).Any();
            if (isEmptyDirectory)
            {
                files = await GetFilteredFiles(dir.GetFiles(), destFiles);
            }

            else files = dir.GetFiles().ToList();

            foreach (FileInfo file in files)
            {
                if (!isEmptyDirectory) Log.Add(new ListBoxItem() { Content = "File " + file.Name + " was added.", Background = Brushes.White });
                string tempPath = Path.Combine(destDirName, file.Name);

                file.CopyTo(tempPath, true);
                ReplaceInFile(tempPath, "namespace " + oldNamespace, "namespace " + newNamespace);
                destFiles = destDir.GetFiles();

                _repository.AddDataReplace(file, tempPath, ComputeMD5Checksum(file.FullName), destFiles.FirstOrDefault(x => x.Name == file.Name), ComputeMD5Checksum(tempPath));
            }
            
            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    await DirectoryCopy(subdir.FullName, temppath, copySubDirs, newNamespace, oldNamespace);
                }
            }

            if (CheckCopiedFolders(sourceDirName, destDirName))
            {
                Log.Add(new ListBoxItem() { Content = "==================================================" , Background = Brushes.PaleGreen });
            }
        }

        private static bool CheckCopiedFolders(string fodler1, string folder2)
        {
            DirectoryInfo dir = new DirectoryInfo(fodler1);
            DirectoryInfo destDir = new DirectoryInfo(folder2);
            DirectoryInfo[] dirs = dir.GetDirectories();
            DirectoryInfo[] destDirs = destDir.GetDirectories();

            List<FileInfo> filesFolder1 = null;
            List<FileInfo> filesFolder2 = null ;

            foreach (var dirFolder in dirs)
            {
                var files = dirFolder.GetFiles().ToList();
                filesFolder1.AddRange(files);
            }

            foreach (var files in destDirs.Select(dirFolder => dirFolder.GetFiles().ToList()))
            {
                filesFolder2.AddRange(files);
            }

            return filesFolder1 == filesFolder2;
        }

        public async Task<List<FileInfo>> GetFilteredFiles(FileInfo[] files, FileInfo[] destFiles)
        {
            var filteredFiles = new List<FileInfo>();
            
            foreach (FileInfo file in files)
            {
                //   if (destFiles.FirstOrDefault(x => x.Name == file.Name) != null) conflictFiles.Add(new ConflictFiles() { FileFromSource = file, FileFromDest = destFiles.FirstOrDefault(x => x.Name == file.Name) });
                if (destFiles.FirstOrDefault(x => x.Name == file.Name) == null)
                {
                    filteredFiles.Add(file);
                    Log.Add(new ListBoxItem() { Content = "File " + file.Name + " has been added.", Background = Brushes.White });
                }
                else if (MergeFile(file, destFiles.FirstOrDefault(x => x.Name == file.Name)))
                {
                    Log.Add(new ListBoxItem() { Content = "File " + file.Name + " need to merge", Background = Brushes.Red });
                    ConflictList.Add(new Conflict() { SourcePath = file.FullName, DestPath = destFiles.FirstOrDefault(x => x.Name == file.Name).FullName });
                }
                else if (NeedReplace(file, destFiles.FirstOrDefault(x => x.Name == file.Name), true))
                {
                    filteredFiles.Add(file);
                    Log.Add(new ListBoxItem() { Content = "File " + file.Name + " has been updated, because was found up to date file...", Background = Brushes.Yellow });
//                    ConflictList.Add(new Conflict() { SourcePath = file.FullName, DestPath = destFiles.FirstOrDefault(x => x.Name == file.Name).FullName });
                }  
                else if (NeedReplace(file, destFiles.FirstOrDefault(x => x.Name == file.Name), false))
                {
                    Log.Add(new ListBoxItem() { Content = "File " + file.Name + " has been added to conflict list, because was found up to date file...", Background = Brushes.Red });
                    ConflictList.Add(new Conflict() { SourcePath = file.FullName, DestPath = destFiles.FirstOrDefault(x => x.Name == file.Name).FullName });
                }
//                if (destFiles.FirstOrDefault(x => x.Name == file.Name) == null || NeedReplace(file, destFiles.FirstOrDefault(x => x.Name == file.Name))) filteredFiles.Add(file);
            }

            foreach (FileInfo file in destFiles)
            {
                if (files.FirstOrDefault(x => x.Name == file.Name) == null)
                {
//                    Log.Add(new ListBoxItem() { Content = "File " + file.Name + " is not in the source folder", Background = Brushes.Yellow });
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

        bool MergeFile(FileInfo file, FileInfo destFile)
        {
            if (_repository.IsDbEmpty())
            {
                var foundFile = _repository.GetFileByPaths(file.FullName, destFile.FullName);
            return !Compare(file, destFile, foundFile);
            }
            return true;
        }
        public bool NeedReplace(FileInfo file, FileInfo destFile, bool isReplace)
        {
            var foundFile = _repository.GetFileByPaths(file.FullName, destFile.FullName);
            if (foundFile != null && isReplace)
                return !Compare(file, foundFile, true);
            if(foundFile != null && !isReplace)
                return !Compare(destFile, foundFile, false);
            return true;
        }

        public async Task<bool> NeedDelete(FileInfo file)
        {
            var FoundFile = await _repository.GetFileByTargetDirectory(file.FullName);
           

//            if (_repository.IsExist(file.FullName))
//            {
//                
//                // Just delete file, because we don't have records about this file...
//                Log.Add(new ListBoxItem() { Content = "Warning! File " + file.FullName + " was deleted, because we don't have records about this file...", Background = Brushes.Red });
//                return true;
//            }              
            

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

        bool Compare(FileInfo sourceFile, FileInfo destFile, DataReplacement foundFile)
        {
            if (sourceFile.LastWriteTime.Ticks == foundFile.Date && sourceFile.Length == foundFile.Size &&
                destFile.LastWriteTime.Ticks == foundFile.DateTarget && destFile.Length == foundFile.Size)
            {
                return true;
            }
            else
            {
                if (sourceFile.Length == foundFile.Size && ComputeMD5Checksum(sourceFile.FullName) == foundFile.Hash &&
                    destFile.Length == foundFile.SizeTarget && ComputeMD5Checksum(destFile.FullName) == foundFile.HashTarget) return true;
            }

            return false;
        }
        bool Compare(FileInfo comparedFile, DataReplacement foundFile, bool isReplace)
        {
            if (isReplace)
            {
                if (comparedFile.LastWriteTime.Ticks == foundFile.Date && comparedFile.Length == foundFile.Size)
                {
                    return true;
                }
                else
                {
                    if (comparedFile.Length == foundFile.Size &&
                        ComputeMD5Checksum(comparedFile.FullName) == foundFile.Hash) return true;
                }

                return false;
            }
            else
            {
                if (comparedFile.LastWriteTime.Ticks == foundFile.DateTarget && comparedFile.Length == foundFile.Size)
                {
                    return true;
                }
                else
                {
                    if (comparedFile.Length == foundFile.SizeTarget && ComputeMD5Checksum(comparedFile.FullName) == foundFile.HashTarget) return true;
                }

                return false;
            }
            
        }


    }

}
