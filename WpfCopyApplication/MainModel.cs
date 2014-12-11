using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using WpfCopyApplication.Model;

namespace WpfCopyApplication
{
    public class MainModel : DependencyObject
    {
        public static readonly DependencyProperty OldNamespaceProperty = DependencyProperty.Register("OldNamespace", typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty SourceDirProperty = DependencyProperty.Register("SourceDir", typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty BackupDirProperty = DependencyProperty.Register("BackupDir", typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty NewNamespaceProperty = DependencyProperty.Register("NewNamespace", typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty ReplaceItemProperty = DependencyProperty.Register("ReplaceItem", typeof(ReplaceItem), typeof(MainModel), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty NameSpaceProperty = DependencyProperty.Register("NameSpace", typeof(string), typeof(MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty PathProperty = DependencyProperty.Register("Path", typeof(string), typeof(MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty NameSpaceTargetProperty = DependencyProperty.Register("NameSpaceTarget", typeof(string), typeof(MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty PathTargetProperty = DependencyProperty.Register("PathTarget", typeof(string), typeof(MainModel), new PropertyMetadata(default(string)));


        private ObservableCollection<ReplaceItem> CollectionReplaceItems  = new ObservableCollection<ReplaceItem>();


        public ObservableCollection<ReplaceItem> _CollectionReplaceItems
        {
            get { return CollectionReplaceItems; }
            set { CollectionReplaceItems = value; }
        }

        public ReplaceItem Item
        {
            get { return new ReplaceItem() { NameSpace = (string)GetValue(NameSpaceProperty), Path = (string)GetValue(PathProperty), NameSpaceTarget = (string)GetValue(NameSpaceTargetProperty), PathTarget = (string)GetValue(PathTargetProperty)};}
            set
            {
                SetValue(ReplaceItemProperty,
                    new ReplaceItem()
                    {
                        NameSpace = (string) GetValue(NameSpaceProperty),
                        Path = (string) GetValue(PathProperty),
                        NameSpaceTarget = (string) GetValue(NameSpaceTargetProperty),
                        PathTarget = (string) GetValue(PathTargetProperty)
                    });
            }
   
        }

        public string OldNamespace
        {
            get { return (string) GetValue(OldNamespaceProperty); }
            set { SetValue(OldNamespaceProperty, value); }
        }

        public string SourceDir
        {
            get { return (string) GetValue(SourceDirProperty); }
            set { SetValue(SourceDirProperty, value); }
        }

        public string BackupDir
        {
            get { return (string) GetValue(BackupDirProperty); }
            set { SetValue(BackupDirProperty, value); }
        }

        public string NewNamespace
        {
            get { return (string) GetValue(NewNamespaceProperty); }
            set { SetValue(NewNamespaceProperty, value); }
        }

        public MainModel(PageAppearanceSection section)
        {
            var defaultData = ConfigurationHelper.ReturnKeys();
            OldNamespace = defaultData.SourceNamespace;
            NewNamespace = defaultData.TargetNamespace;
            SourceDir = defaultData.SourceDirectory;
            BackupDir = defaultData.TargetDirectory;
        }
    }

}