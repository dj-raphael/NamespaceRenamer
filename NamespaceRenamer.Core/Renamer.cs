using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Remoting.Lifetime;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using NamespaceRenamer.Core.Model;

namespace NamespaceRenamer.Core
{
    public class Renamer
    {
        public List<Conflict> ConflictList = new List<Conflict>();
        public IEnumerable<Conflict> ConfList {get { return ConflictList; } } 
        private DataReplacementRepository _repository;

        public List<string> listIgnoreFiles = new List<string>();
        public List<PathAndContent> updateListOfFiles = new List<PathAndContent>();
        public List<PathAndContent> updateListOfCsproj = new List<PathAndContent>();
        public List<PathAndContent> updateListOfSln = new List<PathAndContent>();

        public ConfigManager ConfigList = new ConfigManager();

        public delegate void MethodContainer( Conflict e);
        public event MethodContainer OnAdd = delegate {};

        private string Source;
        private string Target;
        
        public Renamer(ReplaceContext context)
        {
            _repository = new DataReplacementRepository(context); 
        }

        public Renamer() { }

        public async Task ReplaceInFile(string sourcePath, string targetPath, string oldNamespace, string newNamespace, bool needReplace)
        {
            var sourceFile = new FileInfo(sourcePath);
            var targetFile = new FileInfo(targetPath);
            var atr = sourceFile.Attributes;
            var strFile = await ReadFile(sourcePath);

            if (!needReplace)
            {
                strFile = strFile.Replace(oldNamespace, newNamespace);
                await WriteFile(targetPath, strFile, GetFileEncoding(sourcePath));
                // File.WriteAllText(targetPath, strFile, GetFileEncoding(sourcePath));
            }
            else
            {
                await WriteFile(targetPath, strFile, GetFileEncoding(sourcePath));
                // File.Copy(sourcePath, targetPath, true);
            }

            targetFile.Attributes = atr;

            
        }

        private async Task FileCopy(string sourcePath, string targetPath)
        {
            // UnicodeEncoding uniencoding = new UnicodeEncoding();
            // byte[] result = GetFileEncoding(file).GetBytes(file);

            var uniencoding = GetFileEncoding(sourcePath);
            var text = ReadFile(sourcePath).ToString();
            byte[] result = uniencoding.GetBytes(text);

            using (FileStream SourceStream = File.Open(targetPath, FileMode.OpenOrCreate))
            {
                SourceStream.Seek(0, SeekOrigin.End);
                await SourceStream.WriteAsync(result, 0, result.Length);
            }
        }

        private async Task<XmlDocument> XmlReadFile(string fullName)
        {
            var xmlFile = new XmlDocument();
            xmlFile.Load(fullName);

            return xmlFile;
        }

        private async Task<string> ReadFile(string sourcePath)
        {
            using (FileStream sourceStream = new FileStream(sourcePath,FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true))
            {
                StringBuilder sb = new StringBuilder();
                var fileEncoding = GetFileEncoding(sourcePath);

                byte[] buffer = new byte[0x1000];
                int numRead;

                try
                {
                    while ((numRead = await sourceStream.ReadAsync(buffer, 0, buffer.Length)) != 0)
                    {
                        var text = GetFileEncoding(sourcePath).GetString(buffer, 0, numRead);
                        sb.Append(text);
                    }
                }
                catch (FileNotFoundException)
                {
                    var check = sourceStream;
                }

                return sb.ToString();
            }
        }

        private async Task WriteFile(string file, string text, Encoding uniencoding)
        {
            byte[] result = uniencoding.GetBytes(text);

            using (FileStream SourceStream = File.Open(file, FileMode.OpenOrCreate))
            {
                SourceStream.Seek(0, SeekOrigin.Begin);
                await SourceStream.WriteAsync(result, 0, result.Length);
            }
        }


        private async Task XmlWriteFile(string path, XmlDocument xmlContent, Encoding getFileEncoding, string oldNamespace, string newNamespace)
        {
            var x = xmlContent.NamespaceURI;

            xmlContent.Save(path);
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

        //метод для обхода по всем файлам и заполнения updateListOfFiles
        public async Task FillingList(string sourceDirName, List<ConfigFile> updateList,  ProjectReplaceData projectReplace)
        {
            DirectoryInfo sourceDir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = sourceDir.GetDirectories();
            List<FileInfo> files = sourceDir.GetFiles().ToList();

            foreach (FileInfo file in files)
            {
                if (IsExistInList(file, updateList))
                {
                    bool isCsproj;
                    try
                    {
                        isCsproj = file.FullName.Substring(file.FullName.LastIndexOf('.'), 7) == ".csproj";
                    }
                    catch (Exception)
                    {
                        isCsproj = false;
                    }

                    if (isCsproj)
                    {
                        var xmlContent = await XmlReadFile(file.FullName);
                        var replacedFileName = file.Name.Replace(projectReplace.SourceNamespace, projectReplace.TargetNamespace);

                        bool isReplaced = file.Name.Replace(projectReplace.SourceNamespace, projectReplace.TargetNamespace) != file.Name;
                        
                        if (isReplaced)
                        {
                            xmlContent = RenameRootNamespaceAndAssemblyNameInCsproj(xmlContent, projectReplace.SourceNamespace, projectReplace.TargetNamespace);
                            ReplaceCsprojNameInSln(file.Name.Replace(".csproj", ""), replacedFileName.Replace(".csproj", ""));
                            await ReplaceLinksInSln(file.FullName, file.FullName.Replace(projectReplace.SourceNamespace,projectReplace.TargetNamespace), projectReplace.TargetNamespace, projectReplace.SourceNamespace);
                        }

                        updateListOfCsproj.Add(new PathAndContent() { Path = file.FullName, XmlContent = xmlContent, ProjectReplace = projectReplace });
                    }
                    else
                    {
                        bool isSln;
                        try
                        { 
                            isSln = file.FullName.Substring(file.FullName.LastIndexOf('.'), 4) == ".sln";
                        }
                        catch (Exception)
                        {
                            isSln = false;
                        }

                        if (!isSln)
//                            updateListOfSln.Add(new PathAndContent() { Path = file.FullName, Content = await ReadFile(file.FullName), ProjectReplace = projectReplace });
//                        else
                            updateListOfFiles.Add(new PathAndContent() { Path = file.FullName, Content = await ReadFile(file.FullName) });
                    }
                }
            }
            foreach (DirectoryInfo subdir in dirs)
            {
                await FillingList(subdir.FullName, updateList, projectReplace);
            }

        }

        //метод для обхода по всем файлам и заполнения updateListOfFiles
        public async Task FillingListOfSln(string sourceDirName, List<ConfigFile> updateList, ProjectReplaceData projectReplace)
        {
            DirectoryInfo sourceDir = new DirectoryInfo(sourceDirName);
            DirectoryInfo[] dirs = sourceDir.GetDirectories();
            List<FileInfo> files = sourceDir.GetFiles().ToList();

            try
            {
                files = sourceDir.GetFiles().Where(x => (x.FullName.Count() >4 && x.FullName.Remove(0, x.FullName.Count() - 4) == ".sln")).ToList();
            }
            catch (Exception)
            {
                files = null;
            }

            if (files != null) 
                foreach (FileInfo file in files)
                {
                    updateListOfSln.Add(new PathAndContent() { Path = file.FullName, Content = await ReadFile(file.FullName), ProjectReplace = projectReplace });
                }

            foreach (DirectoryInfo subdir in dirs)
            {
                 await  FillingListOfSln(subdir.FullName, updateList, projectReplace);
            }

        }


        public void ReplaceCsprojNameInSln(string oldName, string newName)
        {
            foreach (var file in updateListOfSln)
            {
                    Regex nameOfProject = new Regex(@"\s*=\s*(\""([^\""]*)\"")");
                    for (Match match = nameOfProject.Match(file.Content); match.Success; match = match.NextMatch())
                    {
                        file.Content = file.Content.Replace(match.Value, match.Value.Replace(oldName, newName));
                    }
                    Regex virtualFolder = new Regex(@"(\s*[=]\s*\""\w*\""\,\s\""\w*\"")");
                    for (Match match = virtualFolder.Match(file.Content); match.Success; match = match.NextMatch())
                    {
                        file.Content = file.Content.Replace(match.Value, match.Value.Replace(oldName, newName));
                    }
            }
        }

        public async Task SaveUpdateListOfSln()
        {
            foreach (var file in updateListOfSln)
            {
                file.Path = file.Path.Replace(file.ProjectReplace.SourceDirectory, file.ProjectReplace.TargetDirectory);
                file.Path = file.Path.Replace(file.ProjectReplace.SourceNamespace, file.ProjectReplace.TargetNamespace);

                await WriteFile(file.Path, file.Content, GetFileEncoding(file.Path));
            }
        }

        public async Task SaveUpdateListOfCsproj()
        {
            CheckCsprojName();

            foreach (var csproj in updateListOfCsproj)
            {
                csproj.Path = csproj.Path.Replace(csproj.ProjectReplace.SourceDirectory, csproj.ProjectReplace.TargetDirectory);
                var source = csproj.Path;
                var resultCsproj = new PathAndContent();
                csproj.Path = csproj.Path.Replace(csproj.ProjectReplace.SourceNamespace, csproj.ProjectReplace.TargetNamespace);

                resultCsproj = source != csproj.Path ? RenameProjectReferencesInCsproj(csproj) : csproj;
                csproj.XmlContent = resultCsproj.XmlContent;

                XmlNamespaceManager nsmgr = new XmlNamespaceManager(csproj.XmlContent.NameTable);
                nsmgr.AddNamespace("ab", "http://schemas.microsoft.com/developer/msbuild/2003");

                var nodeListOfCompiles = resultCsproj.XmlContent.SelectNodes("//ab:Project/ab:ItemGroup/ab:Compile", nsmgr);

                if (nodeListOfCompiles != null)
                    foreach (XmlElement node in nodeListOfCompiles)
                    {
                        node.Attributes["Include"].Value = node.Attributes["Include"].Value.Replace(resultCsproj.ProjectReplace.SourceNamespace, resultCsproj.ProjectReplace.TargetNamespace);
                    }

                await XmlWriteFile(resultCsproj.Path, resultCsproj.XmlContent, GetFileEncoding(resultCsproj.Path), resultCsproj.ProjectReplace.SourceNamespace, resultCsproj.ProjectReplace.TargetNamespace);
            }
        }

        public async Task SaveUpdateListOfFiles()
        {
            foreach (var file in updateListOfFiles)
            {
                file.Path = file.Path.Replace(file.ProjectReplace.SourceDirectory, file.ProjectReplace.TargetDirectory);
                file.Path = file.Path.Replace(file.ProjectReplace.SourceNamespace, file.ProjectReplace.TargetNamespace);

                await WriteFile(file.Path, file.Content, GetFileEncoding(file.Path));
            }
        }


        public async Task DirectoryCopy(string sourceDirName, string targetDirName, string newNamespace, string oldNamespace, List<ConfigFile> ignoreList, List<ConfigFile> mandatoryList, bool FirstProcess)
        {
            // If the targetination directory doesn't exist, create it.
            if (!Directory.Exists(targetDirName))
            {
                Directory.CreateDirectory(targetDirName);
            }

            if (FirstProcess)
            {
                Source = sourceDirName;
                Target = targetDirName;
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
                    var conflict = new Conflict()
                    {
                        MessageType = Types.warning,
                        Message = "File " + file.Name + " has been ignored, because file exist in ignore list",
                        BackgroundColor  = "Yellow",
                        ForegroundColor = Brushes.Black 
                    };

                    ConflictList.Add(conflict); 
                    OnAdd(conflict); 
                }
                else if (IsExistInList(file, mandatoryList))
                {
                    string tempPathSource = Path.Combine(sourceDirName, file.Name);

                    var conflict = new Conflict()
                    {
                        MessageType = Types.adding,
                        Message = "File " + file.Name + " has been added, because file exist in mandatory list",
                        ForegroundColor = Brushes.Black
                    };

                    ConflictList.Add(conflict);
                    OnAdd(conflict); 

                    await ReplaceInFile(tempPathSource, tempPath, oldNamespace, newNamespace, false);
                    
                    targetFiles = targetDir.GetFiles();
                    _repository.AddDataReplace(file, tempPath, ComputeMD5Checksum(file.FullName),
                            targetFiles.FirstOrDefault(x => x.Name == file.Name.Replace(oldNamespace, newNamespace)), ComputeMD5Checksum(tempPath));

                    await ReplaceLinksInConfigFiles(tempPathSource, tempPath, newNamespace, oldNamespace);
                    await ReplaceLinksInSln(tempPathSource, tempPath, newNamespace, oldNamespace);
                }
                else
                {
                    if (!notEmptyDirectory)
                    {
                        var conflict = new Conflict()
                        {
                            MessageType = Types.adding,
                            Message = "File " + file.Name + " has been added",
                            ForegroundColor = Brushes.Black
                        };

                            ConflictList.Add(conflict);
                            OnAdd(conflict);
                            
                        }

                    //добавить сообщение о переименовании файла file.Name.Replace(oldNamespace, newNamespace)
                    // file.CopyTo(tempPath, true);
                    await ReplaceInFile(file.FullName, tempPath, oldNamespace, newNamespace, true);
                    
                        targetFiles = targetDir.GetFiles();
                        _repository.AddDataReplace(file, tempPath, ComputeMD5Checksum(file.FullName),
                            targetFiles.FirstOrDefault(x => x.Name == file.Name.Replace(oldNamespace, newNamespace)),
                            ComputeMD5Checksum(tempPath));

                        await ReplaceLinksInConfigFiles(file.FullName, tempPath, newNamespace, oldNamespace);
                        await ReplaceLinksInSln(file.FullName, tempPath, newNamespace, oldNamespace);
                }

            }

            // If copying subdirectories, copy them and their contents to new location.
            foreach (DirectoryInfo subdir in dirs)
            {
                string temppath = Path.Combine(targetDirName, subdir.Name.Replace(oldNamespace, newNamespace));
                await DirectoryCopy(subdir.FullName, temppath, newNamespace, oldNamespace, ignoreList, mandatoryList, false);
            }
        }

        private async Task ReplaceLinksInConfigFiles(string sourcePath, string targetPath, string newNamespace, string oldNamespace)
        {
            string path1 = "", path2 = "";

            // Пройтись по списку и найти где встречаются подобные ссылки
            foreach (var file in updateListOfFiles)
            {
                //Deleting inner Target and Source folders
                var sourcePathWork = sourcePath.Replace(Source + "\\", "");
                var targetPathWork = targetPath.Replace(Target + "\\", "");

                var isCsproj = false;

                string filePath = file.Path;
                filePath = filePath.Replace(Source + "\\", "");

                //Get the part of Path before .config file.
                int position = filePath.LastIndexOf('\\');

                if (position > 0)
                {
                    // filePath = position + 1 < filePath.Length ? filePath.Remove(position + 1) : "";
                    
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

                    try
                    {
                        isCsproj = file.Path.Substring(file.Path.LastIndexOf('.'), 7) != ".csproj";
                    }
                    catch (Exception)
                    {
                        isCsproj = false;
                    }

                    if (isCsproj)
                    {
                        if (sourcePathWork != targetPathWork)
                        {
                            file.Content = file.Content.Replace(sourcePathWork, targetPathWork);
                        }

//                        var doc = new XmlDocument();
//                        doc.Load(file.Path);
//
//                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
//                        nsmgr.AddNamespace("ab", "http://schemas.microsoft.com/developer/msbuild/2003");
//
//                        var node = doc.SelectSingleNode("//ab:Project", nsmgr);
//
//                        replacingChildNodes(node, sourcePathWork, targetPathWork, file);
                    }
                }

                    try
                    {
                        isCsproj = file.Path.Substring(file.Path.LastIndexOf('.'), 7) != ".csproj";
                    }
                    catch (Exception)
                    {
                        isCsproj = false;
                    }

                    if (isCsproj)
                    {
                        if (sourcePathWork != targetPathWork)
                        {
                            file.Content = file.Content.Replace(sourcePathWork, targetPathWork);
                        }

//                        var doc = new XmlDocument();
//                        doc.Load(file.Path);
//
//                        XmlNamespaceManager nsmgr = new XmlNamespaceManager(doc.NameTable);
//                        nsmgr.AddNamespace("ab", "http://schemas.microsoft.com/developer/msbuild/2003");
//
//                        var node = doc.SelectSingleNode("//ab:Project", nsmgr);
//
//                        replacingChildNodes(node, sourcePathWork, targetPathWork, file);
                    }

              }
        }

        private async Task ReplaceLinksInSln(string sourcePath, string targetPath, string newNamespace, string oldNamespace)
        {
            string path1 = "", path2 = "";

            // Пройтись по списку и найти где встречаются подобные ссылки
            foreach (var file in updateListOfSln)
            {
                //Deleting inner Target and Source folders
                var sourcePathWork = sourcePath.Replace(Source + "\\", "");
                var targetPathWork = targetPath.Replace(Target + "\\", "");

                var isCsproj = false;

                string filePath = file.Path;
                filePath = filePath.Replace(Source + "\\", "");

                //Get the part of Path before .config file.
                int position = filePath.LastIndexOf('\\');

                if (position > 0)
                {
                    // filePath = position + 1 < filePath.Length ? filePath.Remove(position + 1) : "";

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

            }
        }

        private void replacingChildNodes(XmlNode node, string sourcePathWork, string targetPathWork, PathAndContent file)
        {
            node.Value = node.Value.Replace(sourcePathWork, targetPathWork);

            XmlNodeList nodes = null;
            if (node.HasChildNodes)
                nodes = node.ChildNodes;

            if (node.Attributes != null)
                foreach (XmlAttribute attr in node.Attributes)
                {
                   attr.Value = attr.Value.Replace(sourcePathWork, targetPathWork);
                }
            
            if (nodes != null)
                foreach (var item in nodes)
                {
                    replacingChildNodes(node, sourcePathWork, targetPathWork, file);
                }
        }

        private bool NotIgnorefile(IEnumerable<ConfigFile> ignoreList, FileInfo file)
        {
            foreach (var ignoreItem in ignoreList)
            {
                if (ignoreItem.IsRegularExpression)
                {
                    Regex regex = new Regex(ignoreItem.XPath.ToLower());
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
                    var mask = ignoreItem.XPath.ToLower();

                    if (path.Contains(mask))
                    {
                       listIgnoreFiles.Add(file.Name);
                       return false;
                    }
                }
             }

            return true;
        }

        public bool IsExistInList(FileInfo file, List<ConfigFile> ignoreInnerReplacingList)
        {

            foreach (var item in ignoreInnerReplacingList)
            {
                if (item.IsRegularExpression)
                {
                    Regex regex = new Regex(item.XPath.ToLower());
                    Match match = regex.Match(file.FullName.ToLower());

                    if (match.Success) return true;
                }
                else
                {
                    if (file.FullName.ToLower().Contains(item.XPath.ToLower())) return true;
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

                    var conflict = new Conflict()
                    {
                        MessageType = Types.conflict,
                        Message = "File " + file.Name + " need to merge",
                        SourcePath = file.FullName,
                        TargetPath = Path.Combine(targetDirName, file.Name),
                        BackgroundColor = "Red",
                        ForegroundColor = Brushes.White
                    };

                    ConflictList.Add(conflict);
                    OnAdd(conflict); 

                }
                else if (FileUpdated(file, targetFiles.FirstOrDefault(x => x.Name == file.Name)))
                {

                    var conflict = new Conflict()
                    {
                        MessageType = Types.warning,
                        Message = "File " + file.Name + " was modified in " + targetDirName,
                        ForegroundColor = Brushes.Black

                    };

                    ConflictList.Add(conflict);
                    OnAdd(conflict); 
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

                        var conflict = new Conflict()
                        {
                            MessageType = Types.warning,
                            Message = "Warning! File " + file.FullName + " was deleted, because file already removed...",
                            ForegroundColor = Brushes.Black,
                            BackgroundColor = "Yellow"
                            
                        };

                        ConflictList.Add(conflict);
                        OnAdd(conflict); 
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
                    _repository.RemoveByHash(FoundFile.Hash, file.FullName);

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

        public async Task Process(ProjectReplaceData item)
        {

            await FillingList(item.SourceDirectory, ConfigList.needUpdateList, item);

            if (IsBlankFolder(item.TargetDirectory))
            {
                string messageBoxText = "The folder is not empty";
                string caption = "";
                System.Windows.Forms.MessageBoxButtons button = MessageBoxButtons.YesNo;
                System.Windows.Forms.MessageBoxIcon icon = MessageBoxIcon.Information;
                DialogResult result = System.Windows.Forms.MessageBox.Show(messageBoxText, caption, button, icon);
                if (result == System.Windows.Forms.DialogResult.Yes)
                    await DirectoryCopy(item.SourceDirectory, item.TargetDirectory, item.TargetNamespace,
                            item.SourceNamespace, ConfigList.ignoreFilesList, ConfigList.mandatoryList, true);
            }
            else
            {
                await DirectoryCopy(item.SourceDirectory, item.TargetDirectory, item.TargetNamespace, item.SourceNamespace,
                        ConfigList.ignoreFilesList, ConfigList.mandatoryList, true);
            }

            await SaveUpdateListOfFiles();

            if (ConflictList.Any() && ConflictList.Last().MessageType != Types.delimiter)
            {
                var conflict = new Conflict()
                {
                    MessageType = Types.warning,
                    Message = "End of the project",
                    ForegroundColor = Brushes.White,
                    BackgroundColor = "Black"
                };

                ConflictList.Add(conflict);
                OnAdd(conflict);
            }

             updateListOfFiles.Clear();
            
        }
        
        public XmlDocument RenameRootNamespaceAndAssemblyNameInCsproj(XmlDocument xmlDocument, string sourceNamespace, string targetNamespace)
        {
                XmlNamespaceManager nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
                nsmgr.AddNamespace("ab", "http://schemas.microsoft.com/developer/msbuild/2003");

                var rootNamespace = xmlDocument.SelectSingleNode("//ab:Project/ab:PropertyGroup/ab:RootNamespace", nsmgr);
                    if (rootNamespace != null)
                        rootNamespace.InnerText = rootNamespace.InnerText.Replace(sourceNamespace, targetNamespace);

                var assemblyName = xmlDocument.SelectSingleNode("//ab:Project/ab:PropertyGroup/ab:AssemblyName", nsmgr);
                    if (assemblyName != null)
                        assemblyName.InnerText = assemblyName.InnerText.Replace(sourceNamespace, targetNamespace);

            return xmlDocument;
        }


        public PathAndContent RenameProjectReferencesInCsproj(PathAndContent csproj)
        {

            XmlNamespaceManager nsmgr = new XmlNamespaceManager(csproj.XmlContent.NameTable);
                nsmgr.AddNamespace("ab", "http://schemas.microsoft.com/developer/msbuild/2003");

            var nameCsproj = csproj.Path.Replace(csproj.ProjectReplace.TargetDirectory, "");
            nameCsproj = nameCsproj.Replace(".csproj", "");

            if (nameCsproj.Contains('\\'))
            {
                int index = nameCsproj.LastIndexOf('\\');

                if (index + 1 < nameCsproj.Length)
                    nameCsproj = nameCsproj.Remove(0, index + 1);
            }

            nameCsproj = nameCsproj.Replace(csproj.ProjectReplace.TargetNamespace, csproj.ProjectReplace.SourceNamespace);

            foreach (var innerCsproj in updateListOfCsproj)
            {
                var nodeListOfProjectReferences = innerCsproj.XmlContent.SelectNodes("//ab:Project/ab:ItemGroup/ab:ProjectReference", nsmgr);
                int q = 0;
                if (nodeListOfProjectReferences != null)
                    foreach (XmlElement node in nodeListOfProjectReferences)
                    {
                        q++;
                        var nameReference = node.SelectSingleNode("./ab:Name", nsmgr);
                        if (nameReference != null && nameReference.InnerText == nameCsproj)
                        {
                            
                            nameReference.InnerText =
                                nameReference.InnerText.Replace(csproj.ProjectReplace.SourceNamespace,
                                    csproj.ProjectReplace.TargetNamespace);
                            node.Attributes["Include"].Value =
                                node.Attributes["Include"].Value.Replace(csproj.ProjectReplace.SourceNamespace,
                                    csproj.ProjectReplace.TargetNamespace);

                            if (nodeListOfProjectReferences.Count == q)
                            {
                                var check = csproj.XmlContent;
                                var check2 = innerCsproj.XmlContent;
                            }
                        }
                    }
             }

            return csproj;
        }

        public void CheckCsprojName()
        {
            foreach (var csproj in updateListOfCsproj)
            {
                csproj.Path = csproj.Path.Replace(csproj.ProjectReplace.SourceDirectory,
                    csproj.ProjectReplace.TargetDirectory);
                var source = csproj.Path;
                
                csproj.Path = csproj.Path.Replace(csproj.ProjectReplace.SourceNamespace,
                    csproj.ProjectReplace.TargetNamespace);

                var resultCsproj = source != csproj.Path ? RenameProjectReferencesInCsproj(csproj) : csproj;
                csproj.XmlContent = resultCsproj.XmlContent;
            }
        }
    }
}
