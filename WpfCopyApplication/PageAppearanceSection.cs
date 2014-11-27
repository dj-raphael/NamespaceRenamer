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
            PageAppearanceSection configuration = ConfigurationManager.GetSection("myConfigurableModule") as PageAppearanceSection;

            if (configuration != null)
                return configuration;

            return new PageAppearanceSection();
        }

        [ConfigurationProperty("namespace",DefaultValue = "Hellow World",IsRequired = false)]
        public string Namespace
        {
            get
            {
                return this["namespace"] as string;
            }
        }

        [ConfigurationProperty("sourceDirectory",DefaultValue = @"C:\")]
        public DirectoryElement SourceDirectory
        {
            get
            {
                return (DirectoryElement)this["sourceDirectory"];
            }
            set
            {
                this["sourceDirectory"] = value;
            }
        }
        [ConfigurationProperty("targetDirectory", DefaultValue = @"C:\")]
        public DirectoryElement TargetDirectory
        {
            get
            {
                return (DirectoryElement)this["targetDirectory"];
            }
            set
            {
                this["targetDirectory"] = value;
            }
        }

        [ConfigurationProperty("sourceNamespace", DefaultValue = "namespace")]
        public NamespaceElement SourceNamespace
        {
            get
            {
                return (NamespaceElement)this["sourceNamespace"];
            }
            set
            { this["sourceNamespace"] = value; }
        }
        [ConfigurationProperty("targetNamespace", DefaultValue = "newNamespace")]
        public NamespaceElement TargetNamespace
        {
            get
            {
                return (NamespaceElement)this["targetNamespace"];
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

        public class DirectoryElement : ConfigurationElement
        {
            [ConfigurationProperty("path", IsRequired = true)]
            [StringValidator(MinLength = 1, MaxLength = 60)]
            public String Path
            {
                get
                {
                    return (String)this["path"];
                }
                set
                {
                    this["path"] = value;
                }
            }
        }
        public class NamespaceElement : ConfigurationElement
        {
            [ConfigurationProperty("namespace", IsRequired = true)]
            [StringValidator(MinLength = 1, MaxLength = 60)]
            public String Namespace
            {
                get
                {
                    return (String)this["namespace"];
                }
                set
                {
                    this["namespace"] = value;
                }
            }
        }

        public class IgnoreCollection : ConfigurationElementCollection
        {
//            public IgnoreCollection()
//            {
//                IgnoreElement element = (IgnoreElement)CreateNewElement();
//                Add(element);
//            }

            public override ConfigurationElementCollectionType CollectionType
            {
                get
                {
//                    return ConfigurationElementCollectionType.AddRemoveClearMap;
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
