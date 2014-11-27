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

namespace WpfCopyApplication
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContext = this;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        // usual OnPropertyChanged implementation

        private string _oldNamespace;
        private string _newNamespace;
        private string _sourceDir;
        private string _backupDir;

        public string OldNamespace
        {
            get { return _oldNamespace; }
            set
            {
                if (value != _oldNamespace)
                {
                    _oldNamespace = value;
                    OnPropertyChanged("OldNamespace");
                }
            }
        }
        public string NewNamespace
        {
            get { return _newNamespace; }
            set
            {
                if (value != _newNamespace)
                {
                    _newNamespace = value;
                    OnPropertyChanged("NewNamespace");
                }
            }
        }
        public string SourceDir
        {
            get { return _sourceDir; }
            set
            {
                if (value != _sourceDir)
                {
                    _sourceDir = value;
                    OnPropertyChanged("SourceDir");
                }
            }
        }
        public string BackupDir
        {
            get { return _backupDir; }
            set
            {
                if (value != _backupDir)
                {
                    _backupDir = value;
                    OnPropertyChanged("BackupDir");
                }
            }
        }
       
        void OnPropertyChanged(string propName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(
                    this, new PropertyChangedEventArgs(propName));
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
            NewNamespace = PageAppearanceSection.GetConfiguration().Namespace;
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
                SourceDir = dlg.FileName;
            }
        }

        private void BrowiseTarget_Click(object sender, RoutedEventArgs e)
        {

            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
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
                BackupDir = dlg.FileName;
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            NewNamespace = PageAppearanceSection.GetConfiguration().Namespace;
            ReplaceNamespace x = new ReplaceNamespace();
            x.CopyFile(SourceDir, BackupDir);
            x.ReplacePartOfFile(BackupDir, OldNamespace, NewNamespace);
        }
    }
}
