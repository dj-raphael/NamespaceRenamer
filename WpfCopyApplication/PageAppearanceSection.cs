using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Configuration;
using System.Windows.Documents;
using System.Xml;

namespace WpfCopyApplication
{
    public class PageAppearanceSection : ConfigurationSection
    {
        public static PageAppearanceSection GetConfiguration()
        {
            var configuration =
                 ConfigurationManager
                 .GetSection("pageAppearance")
                 as PageAppearanceSection;

            return configuration ?? new PageAppearanceSection();
        }

        [ConfigurationProperty("sourceDirectory",DefaultValue = @"C:\")]
        public String SourceDirectory
        {
            get
            {
                return (String)this["sourceDirectory"];
            }
            set
            {
                this["sourceDirectory"] = value;
            }
        }
        [ConfigurationProperty("targetDirectory", DefaultValue = @"C:\")]
        public String TargetDirectory
        {
            get
            {
                return (String)this["targetDirectory"];
            }
            set
            {
                this["targetDirectory"] = value;
            }
        }

        [ConfigurationProperty("sourceNamespace", DefaultValue = "namespace")]
        public String SourceNamespace
        {
            get
            {
                return (String)this["sourceNamespace"];
            }
            set
            { this["sourceNamespace"] = value; }
        }
        [ConfigurationProperty("targetNamespace", DefaultValue = "newNamespace")]
        public String TargetNamespace
        {
            get
            {
                return (String)this["targetNamespace"];
            }
            set
            { this["targetNamespace"] = value; }
        }



        [ConfigurationProperty("ignoreList", IsDefaultCollection = false)]
        [ConfigurationCollection(typeof(IgnoreCollection), CollectionType = ConfigurationElementCollectionType.BasicMapAlternate)]
        public IgnoreCollection IgnoreList
        {
            get
            {
                IgnoreCollection ignoreCollection = (IgnoreCollection)base["ignoreList"];
                return ignoreCollection;
            }
        }

        public class IgnoreCollection : ConfigurationElementCollection
        {
            public override ConfigurationElementCollectionType CollectionType
            {
                get
                {
                    return ConfigurationElementCollectionType.BasicMapAlternate;
                }
            }

            protected override ConfigurationElement CreateNewElement()
            {
                return new IgnoreElement();
            }

            protected override Object GetElementKey(ConfigurationElement element)
            {
                return ((IgnoreElement)element).Value;
            }
        }

        public class IgnoreElement : ConfigurationElement
        {
            [ConfigurationProperty("value", IsRequired = true, IsKey = true)]
            public string Value
            {
                get
                {
                    return (string)this["value"];
                }
                set
                {
                    this["value"] = value;
                }
            }
        }
    }
}
