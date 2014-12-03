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
        static NameValueCollection GetCollection()
        {
            var collection = new NameValueCollection();
            collection.Add("sourceDirectory", @"C:\");
            collection.Add("targetDirectory", @"C:\");
            collection.Add("sourceNamespace", "name1");
            collection.Add("targetNamespace", "name2");
            return collection;

        }

//        static void Main(string[] args)
//        {
//            PrintKey();
//
//            Console.WriteLine("Введите key:");
//            var key = Console.ReadLine();
//            Console.WriteLine("Введите value:");
//            var value = Console.ReadLine();
//
//
//            AddKey(key,value);
//            PrintKey();
//
//            Console.ReadLine();
//        }


        static void AddKey(string sourceDirectory, string targetDirectory, string sourceNamespace, string targetNamespace)
        {
            var xmlDoc = new XmlDocument();
            var ConfigFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
            
            xmlDoc.Load(ConfigFile);

            // <pageAppearance sourceDirectory="C:\Users\alex.ch\Downloads\New folder" targetDirectory ="C:\Users\alex.ch\Downloads\New folder 2" sourceNamespace="name1" targetNamespace="name2">
            // create new node <add key="Region" value="Canterbury" />

            var nodeRegion = xmlDoc.CreateElement("pageAppearance");
            nodeRegion.SetAttribute("sourceDirectory", sourceDirectory );
            nodeRegion.SetAttribute("targetDirectory", targetDirectory );
            nodeRegion.SetAttribute("sourceNamespace", sourceNamespace );
            nodeRegion.SetAttribute("targetNamespace", targetNamespace );

            xmlDoc.AppendChild(nodeRegion);
            xmlDoc.Save(ConfigFile);

            ConfigurationManager.RefreshSection("pageAppearance");
        }


//       public void EditKey()
//       {
//            var xmlDoc = new XmlDocument();
//            var ConfigFile = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
//
//            xmlDoc.Load(ConfigFile);
//
//            xmlDoc.SelectSingleNode("//geoSettings/summary/add[@key='Country']").Attributes["value"].Value = "Old Zeeland";
//            xmlDoc.Save(AppDomain.CurrentDomain.SetupInformation.ConfigurationFile);
//
//            ConfigurationManager.RefreshSection("geoSettings/summary");
//       }


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


        public static string PrintKey(string attr)
        {
           // var pageSettings = ConfigurationManager.AppSettings;
           // var pageSettings = ConfigurationSettings.GetConfig("pageAppearance") as NameValueCollection;
           //  var pageSettings = PageAppearanceSection.GetConfiguration();

             var pageSettings = ConfigurationManager.GetSection("pageAppearance") as NameValueCollection;
             var pageSettings2 = ConfigurationManager.GetSection("pageAppearance");
             


             var test = pageSettings2.ToString();

             return (from key in pageSettings.AllKeys where key == attr select pageSettings[key]).FirstOrDefault();
        }
    }
}

