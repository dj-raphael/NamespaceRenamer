using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WpfCopyApplication.Model
{
    public class DataReplacement
    {
        public string Path { get; set; }
        public long Date { get; set; }
        public long Size { get; set; }
        public string Hash { get; set; }
        public long DateTarget { get; set; }
        public long SizeTarget { get; set; }
        public string HashTarget { get; set; }
        public string PathTargetDirectory { get; set; }
    }
}
