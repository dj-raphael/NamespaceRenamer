using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCopyApplication
{
    public class ReplaceRequest
    {
        public int Id { get; set; }
        public string OldNamespace { get; set; }
        public string SourceDir { get; set; }
        public string NewNamespace { get; set; }
        public string BackupDir { get; set; }
    }
}
