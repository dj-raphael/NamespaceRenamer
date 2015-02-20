using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace NamespaceRenamer
{
    public class ConfigManager
    {
        public List<ProjectReplaceData> projectsList = new List<ProjectReplaceData>();
        public List<ConfigFile> ignoreFilesList = new List<ConfigFile>();
        public List<ConfigFile> mandatoryList = new List<ConfigFile>();
        public List<ConfigFile> needUpdateList = new List<ConfigFile>();
        public string DbPath;

        public  List<ConfigFile> DefaultIgnoreFilesList = new List<ConfigFile>()
                {
                new ConfigFile(){XPath = @"\Bin\", IsRegularExpression = false},
                new ConfigFile(){XPath = @"\obj\", IsRegularExpression = false},
                new ConfigFile(){XPath = @"\.uSEr$", IsRegularExpression = true},
                new ConfigFile(){XPath = @"\.vspscc$", IsRegularExpression = true},
                new ConfigFile(){XPath = @"\.cashe$", IsRegularExpression = true},
                new ConfigFile(){XPath = @"\.vssscc$", IsRegularExpression = true}
            };

        public List<ConfigFile> DefaultMandatoryList = new List<ConfigFile>()
            {
                new ConfigFile(){XPath = @"\.cs$", IsRegularExpression = true}
            };

        public List<ConfigFile> DefaultNeedUpdateList = new List<ConfigFile>()
            {
                new ConfigFile(){XPath = @"\.sln$", IsRegularExpression = true},
                new ConfigFile(){XPath = @"\.csproj$", IsRegularExpression = true},
                new ConfigFile(){XPath = @"\repositories.config$", IsRegularExpression = true}
            };


        public string Load(string configXmlPath)
        {
            projectsList.Clear();
            ignoreFilesList.Clear();
            mandatoryList.Clear();
            needUpdateList.Clear();

            if (string.IsNullOrEmpty(configXmlPath))
            {
                configXmlPath = "Config.xml";
            }

            if (File.Exists(configXmlPath))
            {
                XmlDocument conf = new XmlDocument();
                try
                {
                    conf.Load(configXmlPath);
                }
                catch (Exception xmlException)
                {
                    configXmlPath = "Config.xml";
                    Validate(xmlException);
                   
                }

                var dbNodes = conf.SelectSingleNode("//RenameData/property");
                var projectNodes = conf.SelectNodes("//RenameData/projectsList/project");
                var ignoreNodes = conf.SelectNodes("//RenameData/ignoreList/add");
                var mandatoryNodes = conf.SelectNodes("//RenameData/mandatoryList/add");
                var needUpdatNodes = conf.SelectNodes("//RenameData/needUpdateList/add");

                if (dbNodes != null && dbNodes.Attributes != null) DbPath = dbNodes.Attributes["dataBase"].Value;
                else
                {
                    DbPath = AppDomain.CurrentDomain.BaseDirectory.Substring(0,
                             AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin\\Debug\\")) + "App_Data\\data.sdf";

                    string messageBoxText = "Config.xml doesn't contain path to Database. The DataBase will be created here:" + 
                             AppDomain.CurrentDomain.BaseDirectory.Substring(0, 
                             AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin\\Debug\\")) + "App_Data\\data.sdf";
                        
                    string caption = "";
                    System.Windows.Forms.MessageBoxButtons button = MessageBoxButtons.OK;
                    System.Windows.Forms.MessageBoxIcon icon = MessageBoxIcon.Warning;
                    DialogResult resultConfig = System.Windows.Forms.MessageBox.Show(messageBoxText, caption, button, icon);
                } 
                    
                if (projectNodes != null)
                    foreach (XmlNode item in projectNodes)
                    {
                        if (item.Attributes != null)
                            projectsList.Add(new ProjectReplaceData()
                            {
                                SourceDirectory = item.Attributes["sourceDirectory"].Value,
                                TargetDirectory = item.Attributes["targetDirectory"].Value,
                                SourceNamespace = item.Attributes["sourceNamespace"].Value,
                                TargetNamespace = item.Attributes["targetNamespace"].Value
                            });
                    }

                if (ignoreNodes != null && ignoreNodes.Count != 0)
                    foreach (XmlNode item in ignoreNodes.Cast<XmlNode>().Where(item => item.Attributes != null))
                    {
                        var chack = item.Attributes["isRegularExpression"].Value.ToLower();

                        ignoreFilesList.Add(new ConfigFile()
                        {
                            XPath = item.Attributes["value"].Value,
                            IsRegularExpression = item.Attributes["isRegularExpression"].Value.ToLower() == "true"
                        });
                    }
                else
                {
                    var xmlNodeList = conf.SelectNodes("//RenameData/needUpdateList");
                    if (xmlNodeList != null && xmlNodeList.Count == 0) ignoreFilesList = DefaultIgnoreFilesList;
                }

                if (mandatoryNodes != null && mandatoryNodes.Count != 0)
                    foreach (XmlNode item in mandatoryNodes.Cast<XmlNode>().Where(item => item.Attributes != null))
                    {
                        mandatoryList.Add(new ConfigFile()
                        {
                            XPath = item.Attributes["value"].Value,
                            IsRegularExpression = item.Attributes["isRegularExpression"].Value.ToLower() == "true"
                        });
                    }
                else
                {
                    var xmlNodeList = conf.SelectNodes("//RenameData/mandatoryList");
                    if (xmlNodeList != null && xmlNodeList.Count == 0) mandatoryList = DefaultMandatoryList;
                }

                if (needUpdatNodes != null && needUpdatNodes.Count != 0)
                    foreach (XmlNode item in needUpdatNodes.Cast<XmlNode>().Where(item => item.Attributes != null))
                    {
                        needUpdateList.Add(new ConfigFile()
                        {
                            XPath = item.Attributes["value"].Value,
                            IsRegularExpression = item.Attributes["isRegularExpression"].Value.ToLower() == "true"
                        });
                    }
                else
                {
                    var xmlNodeList = conf.SelectNodes("//RenameData/needUpdateList");
                    if (xmlNodeList != null && xmlNodeList.Count == 0) needUpdateList = DefaultNeedUpdateList;
                }
            }
            else
            {
                DefaultLists();
            }

            return configXmlPath;
        }

        public void Save(string path)
        {
            string aplictionDirectory = "Config.xml";

            if (!string.IsNullOrEmpty(path)) aplictionDirectory = path;
            
            XmlTextWriter textWritter = new XmlTextWriter(aplictionDirectory, Encoding.UTF8);
            textWritter.WriteStartDocument();
            textWritter.WriteStartElement("RenameData");
            textWritter.WriteEndElement();
            textWritter.Close();

            XmlDocument document = new XmlDocument();
            document.Load(aplictionDirectory);
            
            XmlNode propertyTag = AddSubElement(document, "property", document.DocumentElement);
            XmlAttribute dataBase = document.CreateAttribute("dataBase");
            dataBase.Value = DbPath;
            propertyTag.Attributes.Append(dataBase);

            XmlNode projectListTag = AddSubElement(document, "projectsList", document.DocumentElement);
            if (projectsList.Count != 0) 
                foreach (var project in projectsList) 
                    AddSubElement(document, project, projectListTag);

            XmlNode ignoreFilesListTag = AddSubElement(document, "ignoreList", document.DocumentElement);
            if (ignoreFilesList.Count != 0) 
                foreach (var val in ignoreFilesList) 
                    AddSubElement(document, val, ignoreFilesListTag);

            XmlNode mandatoryListTag = AddSubElement(document, "mandatoryList", document.DocumentElement);
            if (mandatoryList.Count != 0)
                foreach (var val in mandatoryList) 
                    AddSubElement(document, val, mandatoryListTag);
            
            XmlNode needUpdateListTag = AddSubElement(document, "needUpdateList", document.DocumentElement);
            if (needUpdateList.Count != 0)
                foreach (var val in needUpdateList) 
                    AddSubElement(document, val, needUpdateListTag);
            
                document.Save(aplictionDirectory);
        }

        private void AddSubElement(XmlDocument document, ProjectReplaceData project, XmlNode parent)
        {
            XmlNode subElement = document.CreateElement("project"); // даём имя
            parent.AppendChild(subElement); // и указываем кому принадлежит
            XmlAttribute sourceDirectoryAtr = document.CreateAttribute("sourceDirectory");
            sourceDirectoryAtr.Value = project.SourceDirectory;
            subElement.Attributes.Append(sourceDirectoryAtr);
            XmlAttribute targetDirectoryAtr = document.CreateAttribute("targetDirectory");
            targetDirectoryAtr.Value = project.TargetDirectory;
            subElement.Attributes.Append(targetDirectoryAtr);
            XmlAttribute sourceNamespaceAtr = document.CreateAttribute("sourceNamespace");
            sourceNamespaceAtr.Value = project.SourceNamespace;
            subElement.Attributes.Append(sourceNamespaceAtr);
            XmlAttribute targetNamespaceAtr = document.CreateAttribute("targetNamespace");
            targetNamespaceAtr.Value = project.TargetNamespace;
            subElement.Attributes.Append(targetNamespaceAtr);
        }
        private void AddSubElement(XmlDocument document, ConfigFile element, XmlNode parent)
        {
            XmlNode subElement = document.CreateElement("add"); // даём имя
            parent.AppendChild(subElement); // и указываем кому принадлежит
            XmlAttribute valueAtr = document.CreateAttribute("value");
            valueAtr.Value = element.XPath;
            subElement.Attributes.Append(valueAtr);
            XmlAttribute isRegularExpressionAtr = document.CreateAttribute("isRegularExpression");
            isRegularExpressionAtr.Value = element.IsRegularExpression.ToString();
            subElement.Attributes.Append(isRegularExpressionAtr);
        }
        private XmlNode AddSubElement(XmlDocument document, string elementName, XmlNode parent)
        {
            XmlNode subElement = document.CreateElement(elementName); // даём имя
            parent.AppendChild(subElement); // и указываем кому принадлежит
            return subElement;
        }

        public void DefaultLists()
        {
            if (ignoreFilesList.Count == 0) ignoreFilesList = DefaultIgnoreFilesList;
            if (mandatoryList.Count == 0) mandatoryList = DefaultMandatoryList;
            if (needUpdateList.Count == 0) needUpdateList = DefaultNeedUpdateList;
        }

        public void Validate(Exception xmlException)
        {
            string messageBoxText = "The config file wasn't read: " + xmlException.Message + "--- Please specify correct config xml file again.";
            string caption = "";
            System.Windows.Forms.MessageBoxButtons button = MessageBoxButtons.OK;
            System.Windows.Forms.MessageBoxIcon icon = MessageBoxIcon.Warning;
            DialogResult resultConfig = System.Windows.Forms.MessageBox.Show(messageBoxText, caption, button, icon);
        }

        public bool Validate()
        {
            if (projectsList.Count < 1)
            {
                var messageBoxText = "Choosed config file doesn't contain data of projects or was wrote incorrectly. You can use application service. After that config file will be rewrited.";
                var caption = "";
                MessageBoxButtons button = MessageBoxButtons.OK;
                MessageBoxIcon icon = MessageBoxIcon.Warning;
                MessageBox.Show(messageBoxText, caption, button, icon);

                return false;
            }

            return true;
        }

        public void LoadBase(string configPath)
        {
            XmlDocument conf = new XmlDocument();
            try
            {
                conf.Load(configPath);
            }
            catch (Exception xmlException)
            {
                configPath = "Config.xml";
                Validate(xmlException);
            }

            var dbNodes = conf.SelectSingleNode("//RenameData/property");

            if (dbNodes != null && dbNodes.Attributes != null)
            {
                DbPath = dbNodes.Attributes["dataBase"].Value;
            }

            if (DbPath != "")
            {
                DbPath = AppDomain.CurrentDomain.BaseDirectory.Substring(0,
                         AppDomain.CurrentDomain.BaseDirectory.IndexOf("bin\\Debug\\", System.StringComparison.Ordinal)) + "App_Data\\data.sdf";
            }

        }
    }
}
