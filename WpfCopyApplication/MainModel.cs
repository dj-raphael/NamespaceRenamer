using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;

namespace WpfCopyApplication
{
    public class MainModel : DependencyObject
    {
        public static readonly DependencyProperty OldNamespaceProperty = DependencyProperty.Register("OldNamespace", typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty SourceDirProperty = DependencyProperty.Register("SourceDir", typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty BackupDirProperty = DependencyProperty.Register("BackupDir", typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));
        public static readonly DependencyProperty NewNamespaceProperty = DependencyProperty.Register("NewNamespace", typeof (string), typeof (MainModel), new PropertyMetadata(default(string)));

        public ObservableCollection<string> Log
        {
            get
            {
                var log = new ObservableCollection<string> {"test1", "test2", "test3"};

                return log;
            }
            set
            {
                var log = new ObservableCollection<string> {"test1", "test2", "test3"};
                log = value;
            }
        }

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
            var DefaultData = ConfigurationHelper.ReturnKeys();
            var test = ReplaceNamespace.getLog();
            OldNamespace = DefaultData.SourceNamespace;
            NewNamespace = DefaultData.TargetNamespace;
            SourceDir = DefaultData.SourceDirectory;
            BackupDir = DefaultData.TargetDirectory;
        }
    }
}