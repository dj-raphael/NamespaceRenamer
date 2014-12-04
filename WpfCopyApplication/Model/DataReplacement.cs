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
        public DateTime Date { get; set; }
        public long Size { get; set; }
        public string Hash { get; set; }
        public string PathTargetDirectory { get; set; }
    }
}
