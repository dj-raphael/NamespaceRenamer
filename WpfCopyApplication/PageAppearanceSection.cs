using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        [ConfigurationProperty("sourceDirectory", DefaultValue = @"C:\")]
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


        [ConfigurationProperty("ignoreList", IsDefaultCollection = false, IsRequired = true)]
        public IgnoreCollection IgnoreList
        {
            get
            {
                return (IgnoreCollection)base["ignoreList"];
            }
        }

        [ConfigurationCollection(typeof(Add), AddItemName = "add")]
        public class IgnoreCollection : ConfigurationElementCollection
        {

            protected override ConfigurationElement CreateNewElement()
            {
                return new Add();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                var l_configElement = element as Add;
                return l_configElement != null ? l_configElement.Value : null;
            }

//            public Add this[int index]
//            {
//                get
//                {
//                    return BaseGet(index) as Add;
//                }
//            }

//            #region IEnumerable<ConfigSubElement> Members
//
//            IEnumerator<Add> IEnumerable<Add>.GetEnumerator()
//            {
//                return (from i in Enumerable.Range(0, this.Count)
//                        select this[i])
//                        .GetEnumerator();
//            }
//
//            #endregion
        }

        public class Add : ConfigurationElement
        {

            [ConfigurationProperty("value", IsKey = true, IsRequired = true)]
            public string Value
            {
                get
                {
                    return base["value"] as string;
                }
                set
                {
                    base["value"] = value;
                }
            }

        }
    }
}
