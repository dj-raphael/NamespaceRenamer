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
        ReplaceContext db = new ReplaceContext();
        private ListBoxOutputter outputter;

        public MainWindow()
        {
            db.Database.Initialize(true);
            InitializeComponent();
            this.DataContext = new MainModel(PageAppearanceSection.GetConfiguration());
            outputter = new ListBoxOutputter(ListBox);
            Console.SetOut(outputter);
            Console.WriteLine("Started");
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
//            var q = PageAppearanceSection.GetConfiguration().IgnoreList;
            ReplaceNamespace x = new ReplaceNamespace(db);
            AddUpdatePrintSection.EditKey(((MainModel)DataContext).SourceDir, ((MainModel)DataContext).BackupDir, ((MainModel)DataContext).NewNamespace, ((MainModel)DataContext).OldNamespace);
            if (!x.IsBlankFolder(((MainModel) DataContext).BackupDir))
            {
                string messageBoxText = "The folder is not empty";
                string caption = "";
                System.Windows.Forms.MessageBoxButtons button = MessageBoxButtons.OKCancel;
                System.Windows.Forms.MessageBoxIcon icon = MessageBoxIcon.Information;
                DialogResult result = System.Windows.Forms.MessageBox.Show(messageBoxText, caption, button, icon);
                if (result == System.Windows.Forms.DialogResult.OK)
                    x.DirectoryCopy(((MainModel) DataContext).SourceDir, ((MainModel) DataContext).BackupDir, true,
                        ((MainModel) DataContext).NewNamespace, ((MainModel) DataContext).OldNamespace);
            }
            else
            {
                x.DirectoryCopy(((MainModel)DataContext).SourceDir, ((MainModel)DataContext).BackupDir, true, ((MainModel)DataContext).NewNamespace, ((MainModel)DataContext).OldNamespace);                
            }
        }
    }
}
