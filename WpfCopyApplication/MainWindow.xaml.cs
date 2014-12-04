using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Windows.Forms;
using WpfCopyApplication.Model;

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
        }

        private void BrowiseSource_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            ((MainModel)DataContext).SourceDir = dialog.SelectedPath;        
        }

        private void BrowiseTarget_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new FolderBrowserDialog();
            DialogResult result = dialog.ShowDialog();
            ((MainModel) DataContext).BackupDir = dialog.SelectedPath;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
//            var q = PageAppearanceSection.GetConfiguration().IgnoreList;
            ReplaceNamespace x = new ReplaceNamespace();
            AddUpdatePrintSection.EditKey(((MainModel)DataContext).SourceDir, ((MainModel)DataContext).BackupDir, ((MainModel)DataContext).NewNamespace, ((MainModel)DataContext).OldNamespace);
            x.DirectoryCopy(((MainModel)DataContext).SourceDir, ((MainModel)DataContext).BackupDir, true, ((MainModel)DataContext).NewNamespace, ((MainModel)DataContext).OldNamespace);
        }
    }
}
