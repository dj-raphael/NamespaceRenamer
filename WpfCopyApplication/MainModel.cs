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
        public static readonly DependencyProperty OldNamespaceProperty = DependencyProperty.Register("OldNamespace",typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty SourceDirProperty = DependencyProperty.Register("SourceDir",typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty BackupDirProperty = DependencyProperty.Register("BackupDir",typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty NewNamespaceProperty = DependencyProperty.Register("NewNamespace",typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty _collectionReplaceItemsProperty = DependencyProperty.Register("collectionReplaceItems", typeof (ObservableCollection<ReplaceItem>),typeof (MyUserControl));

        public ObservableCollection<ReplaceItem> CollectionReplaceItems
        {
            get { return (ObservableCollection<ReplaceItem>) GetValue(_collectionReplaceItemsProperty); }
            set { SetValue(_collectionReplaceItemsProperty, value); }
        }

//        public ObservableCollection<ReplaceItem> _collectionReplaceItems = new ObservableCollection<ReplaceItem>();

//        public ObservableCollection<ReplaceItem> CollectionReplaceItems
//        {
//            get { return _collectionReplaceItems; }
//            set { _collectionReplaceItems = value; }
//        }
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
            var DefaultData = ConfigurationHelper.ReturnKeys();
            OldNamespace = DefaultData.SourceNamespace;
            NewNamespace = DefaultData.TargetNamespace;
            SourceDir = DefaultData.SourceDirectory;
            BackupDir = DefaultData.TargetDirectory;

            var replaceCollection = new ObservableCollection<ReplaceItem>
            {
                new ReplaceItem()
                {
                    SourceDir = DefaultData.SourceDirectory,
                    BackupDir = DefaultData.TargetDirectory,
                    OldNamespace = DefaultData.SourceNamespace,
                    NewNamespace = DefaultData.TargetNamespace
                }
            };

            CollectionReplaceItems = replaceCollection;
        }


    }
}