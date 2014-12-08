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
using WpfCopyApplication.Repository;


namespace WpfCopyApplication
{
    public class ReplaceNamespace
    {
        public static ObservableCollection<ListBoxItem> Log = new ObservableCollection<ListBoxItem>();

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

        public void DirectoryCopy(string sourceDirName, string destDirName, bool copySubDirs, string newNamespace, string oldNamespace)
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

            if (Directory.EnumerateFileSystemEntries(destDirName).Any())
            {
                files = GetFilteredFiles(dir.GetFiles(), destFiles);
            }

            else files = dir.GetFiles().ToList();

            foreach (FileInfo file in files)
            {

                string tempPath = Path.Combine(destDirName, file.Name);
                var tempDestFile = destFiles.FirstOrDefault(x => x.Name == file.Name);
                FileInfo checkFile = new FileInfo(tempPath);
                
                if (checkFile.Exists)
                {
                    Log.Add(new ListBoxItem() { Content = "Warning! File " + tempPath + " was updated, because was found up to date file...", Background = Brushes.Yellow });                        
                }
                else
                {
                    Log.Add(new ListBoxItem() { Content = "File " + tempPath + " was added.", Background = Brushes.White });
                }

                file.CopyTo(tempPath, true);
                ReplaceInFile(tempPath, oldNamespace, newNamespace);      
                _repository.AddDataReplace(file, tempPath, ComputeMD5Checksum(file.FullName));
            }

            // If copying subdirectories, copy them and their contents to new location.
            if (copySubDirs)
            {
                foreach (DirectoryInfo subdir in dirs)
                {
                    string temppath = Path.Combine(destDirName, subdir.Name);
                    DirectoryCopy(subdir.FullName, temppath, copySubDirs, newNamespace, oldNamespace);
                }
            }
        }

        public List<FileInfo> GetFilteredFiles(FileInfo[] files, FileInfo[] destFiles)
        {
            var filteredFiles = new List<FileInfo>();


            foreach (FileInfo file in files)
            {
                //   if (destFiles.FirstOrDefault(x => x.Name == file.Name) != null) conflictFiles.Add(new ConflictFiles() { FileFromSource = file, FileFromDest = destFiles.FirstOrDefault(x => x.Name == file.Name) });
                if (destFiles.FirstOrDefault(x => x.Name == file.Name) == null || NeedReplace(file, destFiles.FirstOrDefault(x => x.Name == file.Name))) filteredFiles.Add(file);

            }

            foreach (FileInfo file in destFiles)
            {
                if (files.FirstOrDefault(x => x.Name == file.Name) == null)
                {
                    if (NeedDelete(file))
                    {
                        file.Delete();
                    }

                    //                 NeedDelete(file);
                    //                 + Нужно сделать проверку с базой:
                    //                 1) Если данные в БД имеются о файле удалить
                    //                 2) Если данные не имеются - добавить в список конфликта и удалить
                }
            }

            return filteredFiles;
        }

        public bool IsBlankFolder(string destDirName)
        {
            DirectoryInfo dir = new DirectoryInfo(destDirName);

            if (!_repository.ConsistRecords(destDirName) && !dir.Exists)
            {
                return true;
            }

            if (!_repository.ConsistRecords(destDirName) && dir.Exists)
            {
                return true;
            }

            return !Directory.EnumerateFileSystemEntries(destDirName).Any();
        }

        public bool NeedReplace(FileInfo file, FileInfo destFile)
        {
            var FoundFile = _repository.GetFileByPaths(file.FullName, destFile.FullName);
            if (FoundFile != null)
                return !Compare(file, FoundFile);  
            return true;
        }

        public bool NeedDelete(FileInfo file)
        {
            bool isExist = _repository.IsExist(file.FullName);
            var FoundFile = _repository.GetFileByTargetDirectory(file.FullName);

            if (_repository.IsExist(file.FullName))
            {
                
                // Just delete file, because we don't have records about this file...
                Log.Add(new ListBoxItem() { Content = "Warning! File " + file.FullName + " was deleted, because we don't have records about this file...", Background = Brushes.Red });
                return true;
            }

            if (_repository.IsExist(file.FullName) && FoundFile.Result.DateTarget == file.LastWriteTime.Ticks && FoundFile.Result.Size == file.Length)
            {
                if (FoundFile.Result.Hash == ComputeMD5Checksum(file.FullName))
                {
                    _repository.RemoveByHash(FoundFile.Result.Hash);
                    Log.Add(new ListBoxItem() { Content = "Warning! File " + file.FullName + " was deleted, because file already removed...", Background = Brushes.Red });
                   
                    return true;
                }
                return true;

            }

            return false;
        }



        private static string ComputeMD5Checksum(string path)
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

        static bool Compare(FileInfo comparedFile, Task<DataReplacement> foundFile)
        {

            if (comparedFile.LastWriteTime.Ticks == foundFile.Result.Date && comparedFile.Length == foundFile.Result.Size)
            {
                return true;
            }
            else
            {
                if (comparedFile.Length == foundFile.Result.Size && ComputeMD5Checksum(comparedFile.FullName) == foundFile.Result.Hash) return true;
            }

            return false;
        }


    }

}
