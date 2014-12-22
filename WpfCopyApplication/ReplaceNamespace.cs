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
using System.Windows.Forms;
using System.Windows.Media;
using WpfCopyApplication.Model;


namespace WpfCopyApplication
{
    public class ReplaceNamespace
    {
        public List<Conflict> ConflictList = new List<Conflict>();
        public IEnumerable<Conflict> ConfList {get { return ConflictList; }} 
        private DataReplacementRepository _repository;
        public ReplaceNamespace(ReplaceContext context)
        {
            _repository = new DataReplacementRepository(context);
        }

        public void ReplaceInFile(string sourcePath, string targetPath, string oldNamespace, string newNamespace)
        {
//          file.CopyTo(sourcePath, true);
            FileInfo sourceFile = new FileInfo(sourcePath);
            FileInfo targetFile = new FileInfo(targetPath);
            var atr = sourceFile.Attributes;
//          targetFile.Attributes &= ~FileAttributes.Hidden;
//            targetFile.Attributes = FileAttributes.Archive;
            String strFile = File.ReadAllText(sourcePath);
            strFile = strFile.Replace(oldNamespace, newNamespace);
            File.WriteAllText(targetPath, strFile);
//          targetFile.Attributes |= FileAttributes.Hidden;
            targetFile.Attributes = atr;
            
        }

        public void AddHistory(ReplaceRequest item)
        {
            _repository.AddHistory(item);        
        }

        public async Task DirectoryCopy(string sourceDirName, string targetDirName, string newNamespace, string oldNamespace)
        {                    
            // Get the subdirectories for the specified directory.
            DirectoryInfo sourceDir = new DirectoryInfo(sourceDirName);
            DirectoryInfo targetDir = new DirectoryInfo(targetDirName);
            DirectoryInfo[] dirs = sourceDir.GetDirectories();

            if (!sourceDir.Exists)
            {
                throw new DirectoryNotFoundException(
                    "Source directory does not exist or could not be found: "
                    + sourceDirName);
            }

            // If the targetination directory doesn't exist, create it.
            if (!Directory.Exists(targetDirName))
            {
                Directory.CreateDirectory(targetDirName);
            }

            // Get the files in the directory and copy them to the new location.
            // FileInfo[] files = dir.GetFiles();

            var targetFiles = targetDir.GetFiles();
            List<FileInfo> files;
            bool notEmptyDirectory = Directory.EnumerateFileSystemEntries(targetDirName).Any();
            if (notEmptyDirectory)
            {
                files = await GetFilteredFiles(sourceDir.GetFiles(), targetFiles, targetDirName);
            }

            else files = sourceDir.GetFiles().ToList();
            
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(targetDirName, file.Name);
                if (!notEmptyDirectory) ConflictList.Add(new Conflict(){ MessageType = Types.adding, Message = "File " + file.Name + " has been added",SourcePath = file.FullName, TargetPath = tempPath});;   
//                file.CopyTo(tempPath, true);
                ReplaceInFile(file.FullName, tempPath, "namespace " + oldNamespace, "namespace " + newNamespace);
                targetFiles = targetDir.GetFiles();
                _repository.AddDataReplace(file, tempPath, ComputeMD5Checksum(file.FullName), targetFiles.FirstOrDefault(x => x.Name == file.Name), ComputeMD5Checksum(tempPath));
            }
            
            // If copying subdirectories, copy them and their contents to new location.
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(targetDirName, subdir.Name);
                    await DirectoryCopy(subdir.FullName, temppath, newNamespace, oldNamespace);
                }
        }

        public async Task<List<FileInfo>> GetFilteredFiles(FileInfo[] files, FileInfo[] targetFiles, string targetDirName)
        {
            var filteredFiles = new List<FileInfo>();
            foreach (FileInfo file in files)
            {
                //   if (targetFiles.FirstOrDefault(x => x.Name == file.Name) != null) conflictFiles.Add(new ConflictFiles() { FileFromSource = file, FileFromtarget = targetFiles.FirstOrDefault(x => x.Name == file.Name) });
                if (targetFiles.FirstOrDefault(x => x.Name == file.Name) == null)
                {
                    filteredFiles.Add(file);
                    ConflictList.Add(new Conflict() { MessageType = Types.adding, Message = "File " + file.Name + " has been added to " + targetDirName, SourcePath = file.FullName, TargetPath = Path.Combine(targetDirName, file.Name) });
                }
                else if (MergeFile(file, targetFiles.FirstOrDefault(x => x.Name == file.Name)))
                {
                    ConflictList.Add(new Conflict() { MessageType = Types.conflict, Message = "File " + file.Name + " need to merge", SourcePath = file.FullName, TargetPath = Path.Combine(targetDirName, file.Name) });
                }
                else if (FileUpdated(file, targetFiles.FirstOrDefault(x => x.Name == file.Name)))
                {
                    ConflictList.Add(new Conflict() { MessageType = Types.warning, Message = "File " + file.Name + " was modified in " + targetDirName, SourcePath = file.FullName, TargetPath = Path.Combine(targetDirName, file.Name) });
                }


            }
                // todo: add filtering by ignore list

            foreach (FileInfo file in targetFiles)
            {
                if (files.FirstOrDefault(x => x.Name == file.Name) == null)
                {
//                    Log.Add(new ListBoxItem() { Content = "File " + file.Name + " is not in the source folder", Background = Brushes.Yellow });
                    if (await NeedDelete(file))
                    {
                        file.Delete();
                        ConflictList.Add(new Conflict() { MessageType = Types.warning, Message = "Warning! File " + file.FullName + " was deleted, because file already removed...", SourcePath = file.FullName, TargetPath = Path.Combine(targetDirName, file.Name) });
                    }

                    //                 NeedDelete(file);
                    //                 + Нужно сделать проверку с базой:
                    //                 1) Если данные в БД имеются о файле удалить
                    //                 2) Если данные не имеются - добавить в список конфликта и удалить
                }
            }

            return filteredFiles;
        }

        public bool IsBlankFolder(string targetDirName)
        {
            DirectoryInfo dir = new DirectoryInfo(targetDirName);

            if (!_repository.ConsistRecords(targetDirName))
            {
                if (dir.Exists)
                {
                    if (Directory.EnumerateFileSystemEntries(targetDirName).Any())
                    {
                        return true;
                    }
                }
                return false;
            }

            return false;

            // return !Directory.EnumerateFileSystemEntries(targetDirName).Any();
        }
        private bool FileUpdated(FileInfo file, FileInfo targetFile)
        {
            if(_repository.GetFileByPaths(file.FullName, targetFile.FullName) == null) return false;
            if (_repository.IsDbEmpty())
            {
                var foundFile = _repository.GetFileByPaths(file.FullName, targetFile.FullName);
                return !Compare(false, targetFile, foundFile);
            }
            return true;

        }
        bool MergeFile(FileInfo file, FileInfo targetFile)
        {
            if (_repository.IsDbEmpty() && _repository.GetFileByPaths(file.FullName, targetFile.FullName) != null)
            {
                var foundFile = _repository.GetFileByPaths(file.FullName, targetFile.FullName);
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
