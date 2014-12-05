using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WpfCopyApplication.Model;
using WpfCopyApplication.Repository;


namespace WpfCopyApplication
{
    public class ReplaceNamespace
    {
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
                file.CopyTo(tempPath, false);
                Console.WriteLine(file.Name + " was copied to the folder: " + tempPath);
                ReplaceInFile(tempPath, oldNamespace, newNamespace);

                _repository.AddDataReplace(file, tempPath);
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
            var conflictFiles = new List<ConflictFiles>();

            foreach (FileInfo file in files)
            {
           //   if (destFiles.FirstOrDefault(x => x.Name == file.Name) != null) conflictFiles.Add(new ConflictFiles() { FileFromSource = file, FileFromDest = destFiles.FirstOrDefault(x => x.Name == file.Name) });
                if (destFiles.FirstOrDefault(x => x.Name == file.Name) == null || _repository.NeedReplace(file, destFiles.FirstOrDefault(x => x.Name == file.Name))) filteredFiles.Add(file);
            }

            foreach (FileInfo file in destFiles)
            {
                if (files.FirstOrDefault(x => x.Name == file.Name) == null )
                {
                    if(_repository.NeedDelete(file)) file.Delete();

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
            // todo: check the folder
            DirectoryInfo dir = new DirectoryInfo(destDirName);
            if (!dir.Exists)
            {
                return true;
            }
            return !Directory.EnumerateFileSystemEntries(destDirName).Any();
        }

    }

    public class ConflictFiles
    {
        public FileInfo FileFromSource { get; set; }
        public FileInfo FileFromDest { get; set; }
    }
}
