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
        public IEnumerable<Conflict> TempConflicts { get; set; }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            if (db.Database.Exists()) db.ReplaceRequests.RemoveRange(db.ReplaceRequests);

            var ignoreList = PageAppearanceSection.GetConfiguration().IgnoreList.OfType<Add>().Select(t => t.Value).ToList();
            var ignoreInnerReplacingList = PageAppearanceSection.GetConfiguration().IgnoreInnerReplacingList.OfType<Add>().Select(t => t.Value).ToList();

            var x = new ReplaceNamespace(db);

            foreach (var item in Model.CollectionReplaceItems)
            {
                if (x.IsBlankFolder(item.TargetDir))
                {
                    string messageBoxText = "The folder is not empty";
                    string caption = "";
                    System.Windows.Forms.MessageBoxButtons button = MessageBoxButtons.YesNo;
                    System.Windows.Forms.MessageBoxIcon icon = MessageBoxIcon.Information;
                    DialogResult result = System.Windows.Forms.MessageBox.Show(messageBoxText, caption, button, icon);
                    if (result == System.Windows.Forms.DialogResult.Yes)
                        await
                            x.DirectoryCopy(item.SourceDir, item.TargetDir, item.NewNamespace, item.OldNamespace, ignoreList, ignoreInnerReplacingList);
                }
                else
                {
                    await
                        x.DirectoryCopy(item.SourceDir, item.TargetDir, item.NewNamespace, item.OldNamespace, ignoreList, ignoreInnerReplacingList);
                }

                x.AddHistory(new ReplaceRequest()
                {
                    NewNamespace = item.NewNamespace,
                    OldNamespace = item.OldNamespace,
                    BackupDir = item.TargetDir,
                    SourceDir = item.SourceDir
                });
                if (x.ConflictList.Any() && x.ConflictList.Last().MessageType != Types.delimiter)
                    x.ConflictList.Add(new Conflict()
                    {
                        MessageType = Types.adding,
                        Message = "End of the project",
                        SourcePath = null,
                        TargetPath = null
                    });
            }
            ListBox.ItemsSource = x.ConfList;
            TempConflicts = x.ConflictList;
            //            Log.ItemsSource = ReplaceNamespace.Log;

        }

        private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange > 0.0)
                ((ScrollViewer) e.OriginalSource).ScrollToEnd();
        }

        private void AddButton_Click(object sender, RoutedEventArgs routedEventArgs)
        {
            var DefaultData = ConfigurationHelper.ReturnKeys();
            ReplaceItem newItem =
                new ReplaceItem()
                {
                    SourceDir = DefaultData.SourceDirectory,
                    TargetDir = DefaultData.TargetDirectory,
                    OldNamespace = DefaultData.SourceNamespace,
                    NewNamespace = DefaultData.TargetNamespace,
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
