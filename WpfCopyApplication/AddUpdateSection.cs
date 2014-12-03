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
    class AddUpdateSection : ConfigurationSection
    {
        static NameValueCollection GetCollection()
        {
            var collection = new NameValueCollection();
            collection.Add("sourceDirectory", @"C:\");
            collection.Add("targetDirectory", @"C:\");
            collection.Add("sourceNamespace", "name1");
            collection.Add("targetNamespace", "name2");
            return collection;
        }

        static void AddKey(string name, string value)
        {
            var xmlDoc = new XmlDocument();
            var ConfigFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            
            xmlDoc.Load(ConfigFile);

            // <pageAppearance sourceDirectory="C:\Users\alex.ch\Downloads\New folder" targetDirectory ="C:\Users\alex.ch\Downloads\New folder 2" sourceNamespace="name1" targetNamespace="name2">
            // create new node <add key="Region" value="Canterbury" />

            var nodeRegion = xmlDoc.CreateElement("pageAppearance");
            nodeRegion.SetAttribute(name, value );
//            nodeRegion.SetAttribute("targetDirectory", targetDirectory );
//            nodeRegion.SetAttribute("sourceNamespace", sourceNamespace );
//            nodeRegion.SetAttribute("targetNamespace", targetNamespace );

            xmlDoc.AppendChild(nodeRegion);
            xmlDoc.Save(ConfigFile);

            ConfigurationManager.RefreshSection("pageAppearance");
        }


       public void EditKey(string key,string attr, string value)
       {
            var xmlDoc = new XmlDocument();
            var ConfigFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

            xmlDoc.Load(ConfigFile);

            xmlDoc.SelectSingleNode("//geoSettings/summary/add[@key='" + key + "']").Attributes[attr].Value = value;
            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);

            ConfigurationManager.RefreshSection("geoSettings/summary");
       }


        public void DeleteAllKeys()
        {
            var xmlDoc = new XmlDocument();
            var ConfigFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;

            xmlDoc.Load(ConfigFile);

            var nodeCity = xmlDoc.SelectSingleNode("//pageAppearance");
            nodeCity.ParentNode.RemoveChild(nodeCity);

            xmlDoc.Save(ConfigFile);
            ConfigurationManager.RefreshSection("//pageAppearance");
        }

        public static void NewConfig(string SourceDir, string BackupDir, string NewNamespace, string OldNamespace)
        {
            
        }
//        public static string PrintKey(string attr)
//        {
////            var pageSettings3 = ConfigurationManager.AppSettings;
////            var pageSettings4 = ConfigurationSettings.GetConfig("pageAppearance") as NameValueCollection;
////            var pageSettings5 = PageAppearanceSection.GetConfiguration();
//
//             var pageSettings = ConfigurationManager.GetSection("pageAppearance") as NameValueCollection;
//             var pageSettings2 = ConfigurationManager.GetSection("pageAppearance");
//          
//             var test = pageSettings2;
//             return (from key in pageSettings.AllKeys where key == attr select pageSettings[key]).FirstOrDefault();
//
//        }
    }
}

