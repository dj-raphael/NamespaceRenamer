﻿using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using Microsoft.Win32;
using System.Windows.Input;
using WpfCopyApplication.Model;
using System.Windows.Forms;

namespace WpfCopyApplication
{
    public partial class MainWindow : Window
    {
        private ReplaceContext db = new ReplaceContext();
        public MainWindow()
        {
            db.Database.Initialize(true);
            InitializeComponent();
            this.Model = new MainModel(PageAppearanceSection.GetConfiguration(), db);
            DataContext = Model;
        }

        public MainModel Model { get; set; }

        private void BrowiseSource_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            Model.SourceDir = dialog.SelectedPath;
        }

        private void BrowiseTarget_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            Model.BackupDir = dialog.SelectedPath;
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            if(db.Database.Exists())  db.ReplaceRequests.RemoveRange(db.ReplaceRequests);
            //  var q = PageAppearanceSection.GetConfiguration().IgnoreList;
            var x = new ReplaceNamespace(db);
            
            ReplaceNamespace.Log.Clear();
            
            foreach (var item in Model.CollectionReplaceItems)
            {
                if (x.IsBlankFolder(item.BackupDir))
                {
                    string messageBoxText = "The folder is not empty";
                    string caption = "";
                    System.Windows.Forms.MessageBoxButtons button = MessageBoxButtons.YesNo;
                    System.Windows.Forms.MessageBoxIcon icon = MessageBoxIcon.Information;
                    DialogResult result = System.Windows.Forms.MessageBox.Show(messageBoxText, caption, button, icon);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                        await x.DirectoryCopy(item.SourceDir, item.BackupDir, item.NewNamespace, item.OldNamespace);
                }
                else
                {
                    await x.DirectoryCopy(item.SourceDir, item.BackupDir, item.NewNamespace, item.OldNamespace);
                }
                
                x.AddHistory(new ReplaceRequest()
                {
                    NewNamespace = item.NewNamespace,
                    OldNamespace = item.OldNamespace,
                    BackupDir = item.BackupDir,
                    SourceDir = item.SourceDir
                });
                if (ReplaceNamespace.Log.Any() && (string)ReplaceNamespace.Log.Last().Content != "==================================================") ReplaceNamespace.Log.Add(new ListBoxItem() { Content = "==================================================", Background = Brushes.PaleGreen });
            }

            Log.ItemsSource = ReplaceNamespace.Log;

        }
        private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange > 0.0)
                ((ScrollViewer)e.OriginalSource).ScrollToEnd();
        }

        private void AddButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            var DefaultData = ConfigurationHelper.ReturnKeys();
            ReplaceItem newItem =
                new ReplaceItem()
                {
                    SourceDir = DefaultData.SourceDirectory,
                    BackupDir = DefaultData.TargetDirectory,
                    OldNamespace = DefaultData.SourceNamespace,
                    NewNamespace = DefaultData.TargetNamespace,
                    Delete = new Command(Model.Delete)
               };
            Model.Add.Execute(newItem);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }

}
