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
            
//            FileInfo[] files = dir.GetFiles();
            


            List<FileInfo> files = GetFilteredFiles(dir.GetFiles());

            foreach (FileInfo file in files)
            {

                string tempPath = Path.Combine(destDirName, file.Name);
                file.CopyTo(tempPath, false);
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

        public List<FileInfo> GetFilteredFiles(FileInfo[] files)
        {
            var filteredFiles = new List<FileInfo>();

            foreach (FileInfo file in files)
            {
                if ( _repository.NeedReplace(file)) filteredFiles.Add(file);
            }

            return filteredFiles;
        }

        public bool IsBlankFolder(string destDirName)
        {
            // todo: check the folder
            DirectoryInfo destDir = new DirectoryInfo(destDirName);
            DirectoryInfo[] destDirs = destDir.GetDirectories();
            if (destDir != null) return false;
            return true;
        }

    }
}
