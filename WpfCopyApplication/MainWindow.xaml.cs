using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using Microsoft.Win32;
using WpfCopyApplication.Model;
using System.Windows.Forms;
using WpfCopyApplication.Repository;

namespace WpfCopyApplication
{
    public partial class MainWindow : Window
    {
        ReplaceContext db = new ReplaceContext();

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
            ((MainModel)DataContext).SourceDir = dialog.SelectedPath;
        }

        private void BrowiseTarget_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            ((MainModel)DataContext).BackupDir = dialog.SelectedPath;
        }

        private async void Start_Click(object sender, RoutedEventArgs e)
        {
            //  var q = PageAppearanceSection.GetConfiguration().IgnoreList;
            var x = new ReplaceNamespace(db);
            await ConfigurationHelper.EditKey(((MainModel)DataContext).SourceDir, ((MainModel)DataContext).BackupDir, ((MainModel)DataContext).NewNamespace, ((MainModel)DataContext).OldNamespace);
            if (!x.IsBlankFolder(((MainModel) DataContext).BackupDir))
            {
                string messageBoxText = "The folder is not empty";
                string caption = "";
                System.Windows.Forms.MessageBoxButtons button = MessageBoxButtons.YesNo;
                System.Windows.Forms.MessageBoxIcon icon = MessageBoxIcon.Information;
                DialogResult result = System.Windows.Forms.MessageBox.Show(messageBoxText, caption, button, icon);
                if (result == System.Windows.Forms.DialogResult.Yes)
                    x.DirectoryCopy(((MainModel) DataContext).SourceDir, ((MainModel) DataContext).BackupDir, true,
                        ((MainModel) DataContext).NewNamespace, ((MainModel) Model).OldNamespace);
            }
            else
            {
                x.DirectoryCopy(((MainModel)DataContext).SourceDir, ((MainModel)DataContext).BackupDir, true, ((MainModel)DataContext).NewNamespace, ((MainModel)DataContext).OldNamespace);                
            }

                Log.ItemsSource = ReplaceNamespace.log;

        }
    }
}
