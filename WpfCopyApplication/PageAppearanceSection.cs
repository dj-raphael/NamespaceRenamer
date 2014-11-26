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
        [ConfigurationProperty("sourceDirectory",DefaultValue = @"C:\")]
        public String DestionDirectory
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
        public String Directory
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

        [ConfigurationProperty("ignoreList", IsDefaultCollection = true)]
        [ConfigurationCollection(typeof(IgnoreCollection),
            AddItemName = "add",
            ClearItemsName = "clear",
            RemoveItemName = "remove")]
        public IgnoreCollection IgnoreList
        {
            get
            {
                IgnoreCollection ignoreCollection =
                    (IgnoreCollection)base["ignoreList"];
                return ignoreCollection;
            }
        }

        public class IgnoreCollection : ConfigurationElementCollection
        {
            public IgnoreCollection()
            {
                string ignoreDirectory = (string)CreateNewElement();
                Add(ignoreDirectory);
            }

            public override ConfigurationElementCollectionType CollectionType
            {
                get
                {
                    return ConfigurationElementCollectionType.AddRemoveClearMap;
                }
            }

            protected override ConfigurationElement CreateNewElement()
            {
                return new UrlConfigElement();
            }

            protected override Object GetElementKey(ConfigurationElement element)
            {
                return ((UrlConfigElement)element).Name;
            }

            public UrlConfigElement this[int index]
            {
                get
                {
                    return (UrlConfigElement)BaseGet(index);
                }
                set
                {
                    if (BaseGet(index) != null)
                    {
                        BaseRemoveAt(index);
                    }
                    BaseAdd(index, value);
                }
            }

            new public UrlConfigElement this[string Name]
            {
                get
                {
                    return (UrlConfigElement)BaseGet(Name);
                }
            }

            public int IndexOf(UrlConfigElement url)
            {
                return BaseIndexOf(url);
            }

            public void Add(UrlConfigElement url)
            {
                BaseAdd(url);
            }
            protected override void BaseAdd(ConfigurationElement element)
            {
                BaseAdd(element, false);
            }

            public void Remove(UrlConfigElement url)
            {
                if (BaseIndexOf(url) >= 0)
                    BaseRemove(url.Name);
            }

            public void RemoveAt(int index)
            {
                BaseRemoveAt(index);
            }

            public void Remove(string name)
            {
                BaseRemove(name);
            }

            public void Clear()
            {
                BaseClear();
            }
        }


//        [ConfigurationCollection("ignoreList")]
//        public List<string> IgnoreList
//        {
//            get
//            {
//                ICollection ignorList = new List<string>()
//                {
//                    @"~\Context\"
//                };
//                return ignorList;
//            }
//            set
//            { this["ignoreList"] = value; }
//        }
    }
}
