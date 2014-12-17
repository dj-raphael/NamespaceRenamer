using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WpfCopyApplication.Model;
using UserControl = System.Windows.Controls.UserControl;

namespace WpfCopyApplication
{
    /// <summary>
    /// Interaction logic for UserControl.xaml
    /// </summary>
    public partial class MyUserControl: UserControl
    {
        public MyUserControl()
        {
            InitializeComponent();

        }

        public static readonly DependencyProperty ModelProperty = DependencyProperty.Register("Model", typeof(ReplaceItem), typeof(MyUserControl), new PropertyMetadata(default(object)));
        public ReplaceItem Model
        {
            get { return (ReplaceItem)GetValue(ModelProperty); }
            set { SetValue(ModelProperty, value); }
        }
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

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Model.Delete.Execute(Model);
        }


    }
}
