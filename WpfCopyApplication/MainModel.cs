using System.Windows;

namespace WpfCopyApplication
{
    public class MainModel : DependencyObject
    {
        public static readonly DependencyProperty OldNamespaceProperty = DependencyProperty.Register("OldNamespace", typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty SourceDirProperty = DependencyProperty.Register("SourceDir", typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty BackupDirProperty = DependencyProperty.Register("BackupDir", typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty NewNamespaceProperty = DependencyProperty.Register("NewNamespace", typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));

        public string OldNamespace
        {
            get { return (string) GetValue(OldNamespaceProperty); }
            set { SetValue(OldNamespaceProperty, value); }
        }

        public string SourceDir
        {
            get { return (string) GetValue(SourceDirProperty); }
            set { SetValue(SourceDirProperty, value); }
        }

        public string BackupDir
        {
            get { return (string) GetValue(BackupDirProperty); }
            set { SetValue(BackupDirProperty, value); }
        }

        public string NewNamespace
        {
            get { return (string) GetValue(NewNamespaceProperty); }
            set { SetValue(NewNamespaceProperty, value); }
        }
        public MainModel(PageAppearanceSection section)
        {
            OldNamespace = section.SourceNamespace;
            NewNamespace = section.TargetNamespace;
            SourceDir = section.SourceDirectory;
            BackupDir = section.TargetDirectory;
        }
    }
}