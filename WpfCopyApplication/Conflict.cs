using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using WpfCopyApplication;
using Brushes = System.Drawing.Brushes;
using Color = System.Drawing.Color;

namespace WpfCopyApplication
{
    public class Conflict
    {
        public string Message { get; set; }
        public Types MessageType { get; set; }
        public string SourcePath { get; set; }
        public string TargetPath { get; set; }
    }
    public enum Types { conflict, warning, adding, delimiter };
}