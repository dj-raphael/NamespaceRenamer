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
//            OpenFileDialog openFileDialog = new OpenFileDialog();
//
//            openFileDialog.InitialDirectory = @"C:\";
//            openFileDialog.Title = "Browse Text Files";
//
//            openFileDialog.CheckFileExists = true;
//            openFileDialog.CheckPathExists = true;
//
//            openFileDialog.DefaultExt = "txt";
//            openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
//            openFileDialog.FilterIndex = 2;
//            openFileDialog.RestoreDirectory = true;
//
//            openFileDialog.ReadOnlyChecked = true;
//            openFileDialog.ShowReadOnly = true;
//
//            if (openFileDialog.ShowDialog() == true)
//            {
//                SourceDir = openFileDialog.FileName;
//            }
            
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.FileName = "Document"; // Default file name
            dlg.DefaultExt = ".txt"; // Default file extension
            dlg.Filter = "Text documents (.txt)|*.txt"; // Filter files by extension 

            // Show open file dialog box
            Nullable<bool> result = dlg.ShowDialog();

            // Process open file dialog box results 
            if (result == true)
            {
                // Open document 
//                string filename = dlg.FileName;
//                SourceDir = dlg.FileName;
            }
        }

        private void BrowiseTarget_Click(object sender, RoutedEventArgs e)
        {
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            var q = PageAppearanceSection.GetConfiguration().IgnoreList;
            ReplaceNamespace x = new ReplaceNamespace();
            AddUpdateSection.NewConfig(((MainModel)DataContext).SourceDir, ((MainModel)DataContext).BackupDir, ((MainModel)DataContext).NewNamespace, ((MainModel)DataContext).OldNamespace);
            x.DirectoryCopy(((MainModel)DataContext).SourceDir, ((MainModel)DataContext).BackupDir, true, ((MainModel)DataContext).NewNamespace, ((MainModel)DataContext).OldNamespace);
        }
    }
}
