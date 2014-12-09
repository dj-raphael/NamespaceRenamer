using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using WpfCopyApplication.Model;
using System.Windows.Forms;
using WpfCopyApplication.Repository;

namespace WpfCopyApplication
{
    public partial class MainWindow : Window
    {
        private ReplaceContext db = new ReplaceContext();

        public MainWindow()
        {
            db.Database.Initialize(true);
            InitializeComponent();
            this.Model = new MainModel(PageAppearanceSection.GetConfiguration());
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
            

            //  var q = PageAppearanceSection.GetConfiguration().IgnoreList;
            var x = new ReplaceNamespace(db);
            ReplaceNamespace.Log.Clear();

            await
                ConfigurationHelper.EditKey(Model.SourceDir, Model.BackupDir,
                    Model.NewNamespace, Model.OldNamespace);
            if (x.IsBlankFolder(Model.BackupDir))
            {
                string messageBoxText = "The folder is not empty";
                string caption = "";
                System.Windows.Forms.MessageBoxButtons button = MessageBoxButtons.YesNo;
                System.Windows.Forms.MessageBoxIcon icon = MessageBoxIcon.Information;
                DialogResult result = System.Windows.Forms.MessageBox.Show(messageBoxText, caption, button, icon);
                if (result == System.Windows.Forms.DialogResult.Yes)
                   await x.DirectoryCopy(Model.SourceDir, Model.BackupDir, true,
                        Model.NewNamespace, Model.OldNamespace);
            }
            else
            {
                await x.DirectoryCopy(Model.SourceDir, Model.BackupDir, true, Model.NewNamespace, Model.OldNamespace);
            }

            
            Log.ItemsSource = ReplaceNamespace.Log;

        }

        private void ListBox_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.ExtentHeightChange > 0.0)
                ((ScrollViewer)e.OriginalSource).ScrollToEnd();
        }

    }

}
