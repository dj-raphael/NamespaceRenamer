using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace WpfCopyApplication.Model
{
    public class ReplaceItem: DependencyObject
    {
        public static readonly DependencyProperty OldNamespaceProperty = DependencyProperty.Register("OldNamespace", typeof (object), typeof (ReplaceItem), new PropertyMetadata(default(object)));
        public static readonly DependencyProperty SourceDirProperty = DependencyProperty.Register("SourceDir", typeof (object), typeof (ReplaceItem), new PropertyMetadata(default(object)));
        public static readonly DependencyProperty NewNamespaceProperty = DependencyProperty.Register("NewNamespace", typeof (object), typeof (ReplaceItem), new PropertyMetadata(default(object)));
        public static readonly DependencyProperty BackupDirProperty = DependencyProperty.Register("BackupDir", typeof (object), typeof (ReplaceItem), new PropertyMetadata(default(object)));

        public object OldNamespace
        {
            get { return (object) GetValue(OldNamespaceProperty); }
            set { SetValue(OldNamespaceProperty, value); }
        }

        public object SourceDir
        {
            get { return (object) GetValue(SourceDirProperty); }
            set { SetValue(SourceDirProperty, value); }
        }

        public object NewNamespace
        {
            get { return (object) GetValue(NewNamespaceProperty); }
            set { SetValue(NewNamespaceProperty, value); }
        }

        public object BackupDir
        {
            get { return (object) GetValue(BackupDirProperty); }
            set { SetValue(BackupDirProperty, value); }
        }
    }
}
