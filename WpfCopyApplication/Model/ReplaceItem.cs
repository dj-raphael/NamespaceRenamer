using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

//namespace WpfCopyApplication.Model
//{
//    public class ReplaceItem
//    {
//        public string NameSpace { get; set; }
//        public string Path { get; set; }
//        public string NameSpaceTarget { get; set; }
//        public string PathTarget { get; set; }
//    }
//}

    
namespace WpfCopyApplication.Model
{
    public class ReplaceItem: DependencyObject
    {
        public static readonly DependencyProperty OldNamespaceProperty = DependencyProperty.Register("OldNamespace", typeof (object), typeof (ReplaceItem), new PropertyMetadata(default(object)));
        public static readonly DependencyProperty SourceDirProperty = DependencyProperty.Register("SourceDir", typeof (object), typeof (ReplaceItem), new PropertyMetadata(default(object)));
        public static readonly DependencyProperty NewNamespaceProperty = DependencyProperty.Register("NewNamespace", typeof (object), typeof (ReplaceItem), new PropertyMetadata(default(object)));
        public static readonly DependencyProperty TargetDirProperty = DependencyProperty.Register("TargetDir", typeof (object), typeof (ReplaceItem), new PropertyMetadata(default(object)));

        public string OldNamespace
        {
            get { return (string)GetValue(OldNamespaceProperty); }
            set { SetValue(OldNamespaceProperty, value); }
        }

        public string SourceDir
        {
            get { return (string)GetValue(SourceDirProperty); }
            set { SetValue(SourceDirProperty, value); }
        }

        public string NewNamespace
        {
            get { return (string)GetValue(NewNamespaceProperty); }
            set { SetValue(NewNamespaceProperty, value); }
        }

        public string TargetDir
        {
            get { return (string) GetValue(TargetDirProperty); }
            set { SetValue(TargetDirProperty, value); }
        }

        public Command Delete { get; set; }
    }
}

