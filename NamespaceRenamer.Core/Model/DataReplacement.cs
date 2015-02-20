using System;
using System.ComponentModel.DataAnnotations;


namespace NamespaceRenamer.Model
{
    public class DataReplacement
    {
        [Key]
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
