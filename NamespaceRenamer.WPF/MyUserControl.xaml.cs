using System.Windows;
using System.Windows.Forms;
using UserControl = System.Windows.Controls.UserControl;

namespace NamespaceRenamer.WPF
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
            Model.TargetDir = dialog.SelectedPath;
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            Model.Delete.Execute(Model);
        }



    }
}
