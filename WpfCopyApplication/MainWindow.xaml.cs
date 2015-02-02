using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NamespaceRenamer.Model;
using System.Windows.Forms;
using NamespaceRenamer;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace WpfCopyApplication
{
    public partial class MainWindow : Window
    {
        private Renamer rename;
        public string configPath, dbPath;
        public Manage Manage = new Manage();

        public MainWindow()
        {
            rename = new Renamer();
            configPath = rename.ConfigList.Load("");
            dbPath = rename.ConfigList.DbPath;
            InitializeComponent();
            this.Model = new MainModel(rename);
            DataContext = Model;
        }

        public MainModel Model { get; set; }
        public IEnumerable<Conflict> TempConflicts { get; set; }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            rename.ConfigList.projectsList.Clear();

            var isCorrectData = true;
            int rowNumber = 1;

            foreach (var item in Model.CollectionReplaceItems)
            {
                if (item.SourceDir == "" || item.TargetDir == "")
                {
                    isCorrectData = false;
                    string messageBoxText = "The launch application was cancelled: In the row №" + rowNumber + " didn't specified directory. Please correct it and try again.";
                    string caption = "";
                    System.Windows.Forms.MessageBoxButtons button = MessageBoxButtons.OK;
                    System.Windows.Forms.MessageBoxIcon icon = MessageBoxIcon.Error;
                    DialogResult result = System.Windows.Forms.MessageBox.Show(messageBoxText, caption, button, icon);
                }

                if (item.OldNamespace == "")
                {
                    isCorrectData = false;
                    string messageBoxText = "The launch application was cancelled: In the row №" + rowNumber + " didn't specified initial data replacement. Please correct it and try again.";
                    string caption = "";
                    System.Windows.Forms.MessageBoxButtons button = MessageBoxButtons.OK;
                    System.Windows.Forms.MessageBoxIcon icon = MessageBoxIcon.Error;
                    DialogResult result = System.Windows.Forms.MessageBox.Show(messageBoxText, caption, button, icon);
                }

                if (Model.CollectionReplaceItems.Any(x => x.SourceDir == item.TargetDir))
                {
                    isCorrectData = false;
                    string messageBoxText = "The launch application was cancelled: Specified the same directory.";
                    string caption = "";
                    System.Windows.Forms.MessageBoxButtons button = MessageBoxButtons.OK;
                    System.Windows.Forms.MessageBoxIcon icon = MessageBoxIcon.Error;
                    DialogResult result = System.Windows.Forms.MessageBox.Show(messageBoxText, caption, button, icon);
                }

                rowNumber++;

                var test =
                rename.ConfigList.projectsList.Any(
                    checkitem =>
                            checkitem.SourceDirectory == item.SourceDir &&
                            checkitem.TargetDirectory == item.TargetDir);

                if (!test)
                {
                    rename.ConfigList.projectsList.Add(new ProjectReplaceData()
                    {
                        SourceDirectory = item.SourceDir,
                        SourceNamespace = item.OldNamespace,
                        TargetDirectory = item.TargetDir,
                        TargetNamespace = item.NewNamespace
                    });
                }
            }

            if (isCorrectData)
            {
                rename.ConfigList.Validate();
                rename.ConfigList.Save(configPath);

                if (configPath != null)
                {
                    if (Manage != null) Manage.Start(configPath);
                }
                else
                {
                    var trouble = true;
                }
            }

            ListBox.ItemsSource = rename.ConfList;
            TempConflicts = rename.ConflictList;
        }

        private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange > 0.0)
                ((ScrollViewer)e.OriginalSource).ScrollToEnd();
        }

        private void AddButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            ReplaceItem newItem =
                new ReplaceItem()
                {
                    SourceDir = "",
                    TargetDir = "",
                    OldNamespace = "",
                    NewNamespace = "",
                    Delete = new Command(Model.Delete)
                };

            Model.Add.Execute(newItem);
        }

        private void StopButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            ListBoxItem item = sender as ListBoxItem;

            ProcessStartInfo pInfo = new ProcessStartInfo("TortoiseMerge.exe");
            pInfo.WorkingDirectory = @"C:\Program Files\TortoiseSVN\bin";
            pInfo.Arguments = "\"" + ((Conflict)item.Content).SourcePath + "\"" + " " + "\"" +
                              ((Conflict)item.Content).TargetPath + "\"" + " \\TortoiseMerge ";
            Process p = Process.Start(pInfo);

            p.WaitForExit();
        }

        private void ConfigButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                InitialDirectory = "c:\\",
                Filter = "config files (*.xml)|*.xml",
                RestoreDirectory = true
            };

            var result = dialog.ShowDialog();
            configPath = dialog.FileName;

            if (dialog.FileName != "")
            {
                configPath = rename.ConfigList.Load(configPath);
                Model.ConfigPath = dialog.FileName;
                Model.DeleteAll();

                if (rename.ConfigList.Validate())
                {
                    foreach (var projectReplace in rename.ConfigList.projectsList)
                    {
                        var newItem = new ReplaceItem()
                        {
                            SourceDir = projectReplace.SourceDirectory,
                            TargetDir = projectReplace.TargetDirectory,
                            OldNamespace = projectReplace.SourceNamespace,
                            NewNamespace = projectReplace.TargetNamespace,
                            Delete = new Command(Model.Delete)
                        };

                        Model.Add.Execute(newItem);
                    }
                }
            }
        }

        private void ConfigTextBox_TextChanged(object sender, MouseButtonEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                InitialDirectory = "c:\\",
                Filter = "config files (*.xml)|*.xml",
                RestoreDirectory = true
            };

            var result = dialog.ShowDialog();
            configPath = dialog.FileName;

            if (dialog.FileName != "")
            {
                configPath = rename.ConfigList.Load(configPath);
                Model.ConfigPath = dialog.FileName;
                Model.DeleteAll();

                if (rename.ConfigList.Validate())
                {
                    foreach (var projectReplace in rename.ConfigList.projectsList)
                    {
                        var newItem = new ReplaceItem()
                        {
                            SourceDir = projectReplace.SourceDirectory,
                            TargetDir = projectReplace.TargetDirectory,
                            OldNamespace = projectReplace.SourceNamespace,
                            NewNamespace = projectReplace.TargetNamespace,
                            Delete = new Command(Model.Delete)
                        };

                        Model.Add.Execute(newItem);
                    }
                }
            }
        }


    }

}
