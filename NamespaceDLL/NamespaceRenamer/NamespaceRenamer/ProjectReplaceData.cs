using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NamespaceRenamer
{
    public class ProjectReplaceData
    {
        public string SourceDirectory { get; set; }
        public string TargetDirectory { get; set; }
        public string SourceNamespace { get; set; }
        public string TargetNamespace { get; set; }
    }
}
