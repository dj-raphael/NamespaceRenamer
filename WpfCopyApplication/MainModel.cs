﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms.VisualStyles;
using System.Windows.Input;
using System.Windows.Media;
using WpfCopyApplication.Model;

namespace WpfCopyApplication
{
    public class MainModel : DependencyObject
    {
        public static readonly DependencyProperty OldNamespaceProperty = DependencyProperty.Register("OldNamespace",typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty SourceDirProperty = DependencyProperty.Register("SourceDir",typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty BackupDirProperty = DependencyProperty.Register("TargetDir",typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty NewNamespaceProperty = DependencyProperty.Register("NewNamespace",typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));

        public static readonly DependencyProperty _collectionReplaceItemsProperty = DependencyProperty.Register("collectionReplaceItems", typeof (ObservableCollection<ReplaceItem>),typeof (MyUserControl));

        public ObservableCollection<ReplaceItem> CollectionReplaceItems
        {
            get { return (ObservableCollection<ReplaceItem>) GetValue(_collectionReplaceItemsProperty); }
            set { SetValue(_collectionReplaceItemsProperty, value); }
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
        public Command Add { get; set; }
        public MainModel(PageAppearanceSection section)
        {
            var DefaultData = ConfigurationHelper.ReturnKeys();
            OldNamespace = DefaultData.SourceNamespace;
            NewNamespace = DefaultData.TargetNamespace;
            SourceDir = DefaultData.SourceDirectory;
            BackupDir = DefaultData.TargetDirectory;
            Add = new Command(AddItem);

            var replaceCollection = new ObservableCollection<ReplaceItem>();
            using (ReplaceContext db = new ReplaceContext())
            {
                db.Database.Initialize(true);
                if (!db.ReplaceRequests.Any())
                {
                    replaceCollection.Add(new ReplaceItem()
                    {
                        SourceDir = DefaultData.SourceDirectory,
                        TargetDir = DefaultData.TargetDirectory,
                        OldNamespace = DefaultData.SourceNamespace,
                        NewNamespace = DefaultData.TargetNamespace,
                        Delete = new Command(Delete)
                    });
                }
                else
                {
                    foreach (var row in db.ReplaceRequests)
                    {
                        ReplaceItem newItem =
                        new ReplaceItem()
                        {
                            SourceDir = row.SourceDir,
                            TargetDir = row.BackupDir,
                            OldNamespace = row.OldNamespace,
                            NewNamespace = row.NewNamespace,
                            Delete = new Command(Delete)
                        };
                        replaceCollection.Add(newItem);
                    }
                }
            }
                      
            CollectionReplaceItems = replaceCollection;
        }

        public void Delete(object param)
        {
            CollectionReplaceItems.Remove((ReplaceItem) param);
        }

        private void AddItem(object param)
        {
            CollectionReplaceItems.Add((ReplaceItem) param);
        }

    }
}