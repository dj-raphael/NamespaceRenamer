using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using NamespaceRenamer;
using WpfCopyApplication.Annotations;

namespace WpfCopyApplication
{
    public class MainModel : DependencyObject
    {
        public static readonly DependencyProperty OldNamespaceProperty = DependencyProperty.Register("OldNamespace",typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty SourceDirProperty = DependencyProperty.Register("SourceDir",typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty BackupDirProperty = DependencyProperty.Register("TargetDir",typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty NewNamespaceProperty = DependencyProperty.Register("NewNamespace",typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty ConfigPathProperty = DependencyProperty.Register("ConfigPathProperty", typeof(string), typeof(MainModel), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty _collectionReplaceItemsProperty = DependencyProperty.Register("collectionReplaceItems", typeof (ObservableCollection<ReplaceItem>),typeof (MyUserControl));

        public ObservableCollection<ReplaceItem> CollectionReplaceItems
        {
            get { return (ObservableCollection<ReplaceItem>) GetValue(_collectionReplaceItemsProperty); }
            set { SetValue(_collectionReplaceItemsProperty, value); }
        }

        public string ConfigPath
        {
            get { return (string)GetValue(ConfigPathProperty); }
            set { SetValue(ConfigPathProperty, value); }            
        }

        public string OldNamespace
        {
            get { return (string)GetValue(OldNamespaceProperty); }
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
        public Command Add { get; set; }
        public MainModel(Renamer rename)
        {
            OldNamespace = "";
            NewNamespace = "";
            SourceDir = "";
            BackupDir = "";
            Add = new Command(AddItem);

            // ReplaceItem.PropertyChanged = Add.CanExecute;

            var replaceCollection = new ObservableCollection<ReplaceItem>();
            

            //если config файл не существует...
            if (!rename.ConfigList.projectsList.Any())
            {
                replaceCollection.Add(new ReplaceItem()
                {
                    SourceDir = "",
                    TargetDir = "",
                    OldNamespace = "",
                    NewNamespace = "",
                    Delete = new Command(Delete)
                });
            }
            else
            {
                foreach (var val in rename.ConfigList.projectsList)
                {
                    ReplaceItem newItem =
                    new ReplaceItem()
                    {
                        SourceDir = val.SourceDirectory,
                        TargetDir = val.TargetDirectory,
                        OldNamespace = val.SourceNamespace,
                        NewNamespace = val.TargetNamespace,
                        Delete = new Command(Delete)
                    };
                    replaceCollection.Add(newItem);
                }
            }

            

            CollectionReplaceItems = replaceCollection;
        }

        public void Delete(object param)
        {
            CollectionReplaceItems.Remove((ReplaceItem) param);
        }

        public void DeleteAll()
        {
            CollectionReplaceItems.Clear();
        }

        private void AddItem(object param)
        {
            CollectionReplaceItems.Add((ReplaceItem) param);
        }

        public void AddProjectInProjectList()
        {
            
        }

    }
}