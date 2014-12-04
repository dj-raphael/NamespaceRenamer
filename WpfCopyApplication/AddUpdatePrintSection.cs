using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace WpfCopyApplication
{
    class AddUpdatePrintSection : ConfigurationSection
    {
        public static void EditKey(string sourceDir, string backupDir, string newNamespace, string oldNamespace)
        {
            var xmlDoc = new XmlDocument();
            var ConfigFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

            xmlDoc.Load(ConfigFile);

            xmlDoc.SelectSingleNode("//pageAppearance[1]").Attributes["sourceDirectory"].Value = sourceDir;
            xmlDoc.SelectSingleNode("//pageAppearance[1]").Attributes["targetDirectory"].Value = backupDir;
            xmlDoc.SelectSingleNode("//pageAppearance[1]").Attributes["sourceNamespace"].Value = oldNamespace;
            xmlDoc.SelectSingleNode("//pageAppearance[1]").Attributes["targetNamespace"].Value = newNamespace;

            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            ConfigurationManager.RefreshSection("pageAppearance");
        }

        public static void ReturnKeys()
        {
            var xmlDoc = new XmlDocument();
            var pageAppearance = new PageAppearanceSection();
            var ConfigFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

            xmlDoc.Load(ConfigFile);
            pageAppearance.SourceDirectory = xmlDoc.SelectSingleNode("//pageAppearance[1]").Attributes["sourceDirectory"].Value.ToString();
            pageAppearance.SourceDirectory = xmlDoc.SelectSingleNode("//pageAppearance[1]").Attributes["targetDirectory"].Value.ToString();
            pageAppearance.SourceDirectory = xmlDoc.SelectSingleNode("//pageAppearance[1]").Attributes["sourceNamespace"].Value.ToString();
            pageAppearance.SourceDirectory = xmlDoc.SelectSingleNode("//pageAppearance[1]").Attributes["targetNamespace"].Value.ToString();
        }
    }
}

