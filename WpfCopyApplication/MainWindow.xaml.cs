using System;
using System.Windows;
using Microsoft.Win32;
using WpfCopyApplication.Model;
using System.Windows.Forms;
using WpfCopyApplication.Repository;

namespace WpfCopyApplication
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            
            using (var db = new ReplaceContext())
            {
                db.Database.Initialize(true);
            }

            InitializeComponent();
            this.DataContext = new MainModel(PageAppearanceSection.GetConfiguration());
            var repository = new DataReplacementRepository();
        }

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

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            var q = PageAppearanceSection.GetConfiguration().IgnoreList;
            ReplaceNamespace x = new ReplaceNamespace();

            AddUpdatePrintSection.EditKey(((MainModel)DataContext).SourceDir, ((MainModel)DataContext).BackupDir, ((MainModel)DataContext).NewNamespace, ((MainModel)DataContext).OldNamespace);
            x.DirectoryCopy(((MainModel)DataContext).SourceDir, ((MainModel)DataContext).BackupDir, true, ((MainModel)DataContext).NewNamespace, ((MainModel)DataContext).OldNamespace);

        }
    }
}
