using System.ComponentModel;
using System.Runtime.CompilerServices;

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
using WpfCopyApplication.Annotations;

namespace WpfCopyApplication
{
    public class ReplaceItem: INotifyPropertyChanged
    {
        private string _oldNamespace;
        private string _sourceDir;
        private string _newNamespace;
        private string _targetDir;

        public string OldNamespace
        {
            get { return _oldNamespace; }
            set
            {
                if (value == _oldNamespace) return;
                _oldNamespace = value;
                OnPropertyChanged();
            }
        }

        public string SourceDir
        {
            get { return _sourceDir; }
            set
            {
                if (value == _sourceDir) return;
                _sourceDir = value;
                OnPropertyChanged();
            }
        }

        public string NewNamespace
        {
            get { return _newNamespace; }
            set
            {
                if (value == _newNamespace) return;
                _newNamespace = value;
                OnPropertyChanged();
            }
        }

        public string TargetDir
        {
            get { return _targetDir; }
            set
            {
                if (value == _targetDir) return;
                _targetDir = value;
                OnPropertyChanged();
            }
        }

        public Command Delete { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        
    }
}

