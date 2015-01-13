using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Globalization;
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

        public List<string> listIgnoreFiles = new List<string>();
        public List<PathAndContent> updateListOfFiles = new List<PathAndContent>();
        
        public string Source;
        public string Target;
        
        public ReplaceNamespace(ReplaceContext context)
        {
            _repository = new DataReplacementRepository(context);
        }

        public void ReplaceInFile(string sourcePath, string targetPath, string oldNamespace, string newNamespace, bool NeedReplace)
        {

            FileInfo sourceFile = new FileInfo(sourcePath);
            FileInfo targetFile = new FileInfo(targetPath);
            var atr = sourceFile.Attributes;
            String strFile = File.ReadAllText(sourcePath);

            if (!NeedReplace)
            {
                strFile = strFile.Replace(oldNamespace, newNamespace);
                File.WriteAllText(targetPath, strFile, GetFileEncoding(sourcePath));
            }
            else
            {
                File.Copy(sourcePath, targetPath);
            }

            targetFile.Attributes = atr;
        }

        public static Encoding GetFileEncoding(String FileName)
        {
            Encoding Result = null;
            FileInfo FI = new FileInfo(FileName);
            FileStream FS = null;

            try
            {
                FS = FI.OpenRead();
                Encoding[] UnicodeEncodings = { Encoding.BigEndianUnicode, Encoding.Unicode, Encoding.UTF8 };
                for (int i = 0; Result == null && i < UnicodeEncodings.Length; i++)
                {
                    FS.Position = 0;
                    byte[] Preamble = UnicodeEncodings[i].GetPreamble();
                    bool PreamblesAreEqual = true;
                    for (int j = 0; PreamblesAreEqual && j < Preamble.Length; j++)
                    {
                        PreamblesAreEqual = Preamble[j] == FS.ReadByte();
                    }
                    if (PreamblesAreEqual)
                    {
                        Result = UnicodeEncodings[i];
                    }
                }
            }
            catch (System.IO.IOException)
            {
            }
            finally
            {
                if (FS != null)
                {
                    FS.Close();
                }
            }

            if (Result == null)
            {
                Result = Encoding.Default;
            }

            return Result;
        }

        public void AddHistory(ReplaceRequest item)
        {
            _repository.AddHistory(item);     
        }

        //метод для обхода по всем файлам и заполнения updateListOfFiles
        public async Task FillingList(string sourceDirName, List<Add> updateList)
        {
            DirectoryInfo sourceDir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = sourceDir.GetDirectories();
            List<FileInfo> files = sourceDir.GetFiles().ToList();
            foreach (FileInfo file in files)
            {
                if (IsExistInList(file, updateList))
                {
                    updateListOfFiles.Add(new PathAndContent(){Path = file.FullName, Content = File.ReadAllText(file.FullName) });
                }
            }
            foreach (DirectoryInfo subdir in dirs)
            {
                await FillingList(subdir.FullName, updateList);
            }
        }

        public async Task SaveUpdateListOfFiles( string oldNamespace, string newNamespace, string source, string target)
        {
            foreach (var file in updateListOfFiles)
            {
                if (file.Path.Substring(file.Path.LastIndexOf('.'), 4) == ".sln")
                {
                    Regex nameOfProject = new Regex(@"\s*=\s*(\""([^\""]*)\"")");
                    for (Match match = nameOfProject.Match(file.Content); match.Success; match = match.NextMatch())
                    {
                        file.Content = file.Content.Replace(match.Value, match.Value.Replace(oldNamespace, newNamespace));
                    }
                    Regex virtualFolder = new Regex(@"(\s*[=]\s*\""\w*\""\,\s\""\w*\"")");
                    for (Match match = virtualFolder.Match(file.Content); match.Success; match = match.NextMatch())
                    {
                        file.Content = file.Content.Replace(match.Value, match.Value.Replace(oldNamespace, newNamespace));
                    }
                }
                File.WriteAllText(file.Path.Replace(oldNamespace, newNamespace).Replace(source, target), file.Content, GetFileEncoding(file.Path));
            }
        }

        public async Task DirectoryCopy(string sourceDirName, string targetDirName, string newNamespace, string oldNamespace, List<Add> ignoreList, List<Add> ignoreInnerReplacingList)
        {
            // If the targetination directory doesn't exist, create it.
            if (!Directory.Exists(targetDirName))
            {
                Directory.CreateDirectory(targetDirName);
            }

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

            // Get the files in the directory and copy them to the new location.
            // FileInfo[] files = dir.GetFiles();

            var targetFiles = targetDir.GetFiles();
            var sourceFiles = sourceDir.GetFiles().ToList();

            List<FileInfo> files;
            bool notEmptyDirectory = Directory.EnumerateFileSystemEntries(targetDirName).Any();

            if (notEmptyDirectory)
            {
                files = await GetFilteredFiles(sourceDir.GetFiles(), targetFiles, targetDirName);
            }
            else files = sourceFiles;
            
            foreach (FileInfo file in files)
            {
                string tempPath = Path.Combine(targetDirName, file.Name.Replace(oldNamespace, newNamespace));

                if (!NotIgnorefile(ignoreList, file))
                {
                    ConflictList.Add(new Conflict()
                    {
                        MessageType = Types.adding,
                        Message = "File " + file.Name + " has been ignored, because file exist in ignore List",
                        SourcePath = file.FullName,
                        TargetPath = tempPath
                    });
                }
                else if (IsExistInList(file, ignoreInnerReplacingList))
                {
                    string tempPathSource = Path.Combine(sourceDirName, file.Name);

                    ConflictList.Add(new Conflict()
                    {
                        MessageType = Types.adding,
                        Message = "File " + file.Name + " has been added, because file exist in ignoreInnerCoping ",
                        SourcePath = tempPathSource,
                        TargetPath = tempPath
                    });
                    //await Task.Delay(100);
                    ReplaceInFile(tempPathSource, tempPath, oldNamespace, newNamespace, false);
                    targetFiles = targetDir.GetFiles();
                    _repository.AddDataReplace(file, tempPath, ComputeMD5Checksum(file.FullName),
                        targetFiles.FirstOrDefault(x => x.Name == file.Name.Replace(oldNamespace, newNamespace)), ComputeMD5Checksum(tempPath));
                    ReplaceLinks(tempPathSource, tempPath, newNamespace, oldNamespace);
                }
                else
                {
                    if (!notEmptyDirectory)
                        ConflictList.Add(new Conflict()
                        {
                            MessageType = Types.adding,
                            Message = "File " + file.Name + " has been added",
                            SourcePath = file.FullName,
                            TargetPath = tempPath
                        });
                    //добавить сообщение о переименовании файла file.Name.Replace(oldNamespace, newNamespace)
                    // file.CopyTo(tempPath, true);
                    ReplaceInFile(file.FullName, tempPath, oldNamespace, newNamespace, true);
                    targetFiles = targetDir.GetFiles();
                    _repository.AddDataReplace(file, tempPath, ComputeMD5Checksum(file.FullName),
                        targetFiles.FirstOrDefault(x => x.Name == file.Name.Replace(oldNamespace, newNamespace)), ComputeMD5Checksum(tempPath));
                    ReplaceLinks(file.FullName, tempPath, newNamespace, oldNamespace);
                }
            }


            // If copying subdirectories, copy them and their contents to new location.
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(targetDirName, subdir.Name.Replace(oldNamespace, newNamespace));
                await DirectoryCopy(subdir.FullName, temppath, newNamespace, oldNamespace, ignoreList, ignoreInnerReplacingList);
            }
        }

        private void ReplaceLinks(string sourcePath, string targetPath, string newNamespace, string oldNamespace)
        {
            string partBeforeConfig, relativeSourcePath, relativeTargetPath, targetFilePath;
            string path1 = "", path2 = "";
            var paths = new List<string>();
            var ConfigFolders = new List<string>();

            //пройтись по списку и найти где встречаются подобные ссылки
            foreach (var file in updateListOfFiles)
            {
                //Deleting inner Target and Source folders
                var sourcePathWork = sourcePath.Replace(Source + "\\", "");
                var targetPathWork = targetPath.Replace(Target + "\\", "");

                string filePath = file.Path;
                filePath = filePath.Replace(Source + "\\", "");

                //Get the part of Path before .config file.
                int position = filePath.LastIndexOf('\\');

                if (position > 0)
                {
//                    filePath = position + 1 < filePath.Length ? filePath.Remove(position + 1) : "";
                    
                    int count1 = filePath.IndexOf('\\');
                    int count2 = sourcePathWork.IndexOf('\\');

                    if (count1 > 0 && count2 > 0)
                    {
                        path1 = count1 + 1 < filePath.Length ? filePath.Remove(count1 + 1) : "";
                        path2 = count2 + 1 < sourcePathWork.Length ? sourcePathWork.Remove(count2 + 1) : "";
                    }

                    while (count1 > 0 && count2 > 0 && path1 == path2 && path1 != "")
                    {
                        filePath = filePath.Remove(0, count1 + 1);
                        sourcePathWork = sourcePathWork.Remove(0, count2 + 1);

                        if (path1 == path2 && path1 != "")
                        {
                            targetPathWork = targetPathWork.Replace(path2.Replace(oldNamespace, newNamespace), "");
                        }

                        count1 = filePath.IndexOf('\\');
                        count2 = sourcePathWork.IndexOf('\\');

                        

                        if (count1 > 0 && count2 > 0)
                        {
                            path1 = count1 + 1 < filePath.Length ? filePath.Remove(count1 + 1) : "";
                            path2 = count2 + 1 < sourcePathWork.Length ? sourcePathWork.Remove(count2 + 1) : "";
                        }
                    }

                    if (sourcePathWork != targetPathWork)
                    {
                        file.Content = file.Content.Replace(sourcePathWork, targetPathWork);
                    }

                }



                if (sourcePathWork != targetPathWork)
                {
                    file.Content = file.Content.Replace(sourcePathWork, targetPathWork);
                }

//                if(sourcePath.LastIndexOf('\\') >= file.Path.LastIndexOf('\\'))
//                {
//                    position = file.Path.LastIndexOf('\\');
//                    pathToFile = file.Path.Remove(position);
//                    count = pathToFile.Length + 1;
//                    relativeSourcePath = sourcePath.Remove(0, count);
//
//                    targetFilePath = file.Path.Replace(oldNamespace, newNamespace);
//                    position = targetFilePath.LastIndexOf('\\');
//                    pathToFile = targetFilePath.Remove(position);
//                    count = pathToFile.Length + 1;
//                    relativeTargetPath = targetPath.Remove(0, count);
//
//                    
//                }       
       
            }
        }

        private bool NotIgnorefile(IEnumerable<Add> ignoreList, FileInfo file)
        {
            foreach (var ignoreItem in ignoreList)
            {


                if (ignoreItem.IsRegularExpression)
                {
                    Regex regex = new Regex(ignoreItem.Value.ToLower());
                    Match match = regex.Match(file.FullName.ToLower());

                    if (match.Success)
                    {
                        listIgnoreFiles.Add(file.Name);
                        return false;
                    } 
                }
                else
                {
                    var path = file.FullName.ToLower();
                    var mask = ignoreItem.Value.ToLower();

                    if (path.Contains(mask))
                    {
                       listIgnoreFiles.Add(file.Name);
                       return false;
                    }
                }
             }

            return true;
        }

        public bool IsExistInList(FileInfo file, List<Add> ignoreInnerReplacingList)
        {

            foreach (var item in ignoreInnerReplacingList)
            {
                if (item.IsRegularExpression)
                {
                    Regex regex = new Regex(item.Value.ToLower());
                    Match match = regex.Match(file.FullName.ToLower());

                    if (match.Success) return true;
                }
                else
                {
                    if (file.FullName.ToLower().Contains(item.Value.ToLower())) return true;
                }
            }
            return false;
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

            foreach (FileInfo file in targetFiles)
            {
                if (files.FirstOrDefault(x => x.Name == file.Name) == null)
                {
                    //  Log.Add(new ListBoxItem() { Content = "File " + file.Name + " is not in the source folder", Background = Brushes.Yellow });
                    if (await NeedDelete(file))
                    {
                        file.Delete();
                        ConflictList.Add(new Conflict() { MessageType = Types.warning, Message = "Warning! File " + file.FullName + " was deleted, because file already removed...", SourcePath = file.FullName, TargetPath = Path.Combine(targetDirName, file.Name) });
                    }
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
