using System.Drawing;

namespace NamespaceRenamer.Core
{
    public class Conflict
    {
        public string Message { get; set; }
        public Types MessageType { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
        public bool Merge { get; set;  }
        public string  BackgroundColor { get; set; }
        public Brush ForegroundColor { get; set; }
    }
    public enum Types { conflict, warning, adding, delimiter };
}