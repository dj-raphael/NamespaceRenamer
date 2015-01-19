using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.VisualStyles;
using System.Windows.Media;
using Microsoft.Win32;
using System.Windows.Input;
using NamespaceRenamer.Model;
using System.Windows.Forms;
using NamespaceRenamer;

namespace WpfCopyApplication
{
    public partial class MainWindow : Window
    {
        private ReplaceContext db = new ReplaceContext();
        private Renamer rename;
        public MainWindow()
        {
            rename = new Renamer(db);
            db.Database.Initialize(true);
            InitializeComponent();
            rename.ReadXml("");
            this.Model = new MainModel(rename);
            DataContext = Model;
        }

        public MainModel Model { get; set; }
        public IEnumerable<Conflict> TempConflicts { get; set; }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            var ignoreList = rename.ignoreFilesList;
            var mandatoryList = rename.mandatoryList;
            var needUpdateList = rename.needUpdateList;

            foreach (var item in Model.CollectionReplaceItems)
            {
                rename.projectsList.Add(new ProjectReplaceData()
                {
                    SourceDirectory = item.SourceDir,
                    SourceNamespace = item.OldNamespace,
                    TargetDirectory = item.TargetDir,
                    TargetNamespace = item.NewNamespace
                });
                rename.AddToXMLConfig();
                    await rename.FillingList(item.SourceDir, needUpdateList);
                
                if (rename.IsBlankFolder(item.TargetDir))
                {
                    string messageBoxText = "The folder is not empty";
                    string caption = "";
                    System.Windows.Forms.MessageBoxButtons button = MessageBoxButtons.YesNo;
                    System.Windows.Forms.MessageBoxIcon icon = MessageBoxIcon.Information;
                    DialogResult result = System.Windows.Forms.MessageBox.Show(messageBoxText, caption, button, icon);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                        await rename.DirectoryCopy(item.SourceDir, item.TargetDir, item.NewNamespace, item.OldNamespace, ignoreList, mandatoryList, true);
                }
                else
                {
                    await rename.DirectoryCopy(item.SourceDir, item.TargetDir, item.NewNamespace, item.OldNamespace, ignoreList, mandatoryList, true);
                }

                await rename.SaveUpdateListOfFiles(item.OldNamespace, item.NewNamespace, item.SourceDir, item.TargetDir);



                if (rename.ConflictList.Any() && rename.ConflictList.Last().MessageType != Types.delimiter)
                    rename.ConflictList.Add(new Conflict()
                    {
                        MessageType = Types.adding,
                        Message = "End of the project",
                        SourcePath = null,
                        TargetPath = null
                    });
            }
            ListBox.ItemsSource = rename.ConfList;
            TempConflicts = rename.ConflictList;
            //            Log.ItemsSource = ReplaceNamespace.Log;

        }

        private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange > 0.0)
                ((ScrollViewer) e.OriginalSource).ScrollToEnd();
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
            pInfo.Arguments = "\"" + ((Conflict) item.Content).SourcePath + "\"" + " " + "\"" +
                              ((Conflict) item.Content).TargetPath + "\"" + " \\TortoiseMerge ";
            Process p = Process.Start(pInfo);

            p.WaitForExit();


            //            if (((Conflict) item.Content).MessageType == Types.conflict)
            //            {
            //                MergeWindow win = new MergeWindow((Conflict)item.Content);
            //                win.Show();
            //            }
        }
    }

}
