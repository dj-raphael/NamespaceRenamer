using System.Xml;

namespace NamespaceRenamer.Core
{
    public class PathAndContent
    {
        public string Path { get; set; }
        public string Content { get; set; }
        public XmlDocument XmlContent { get; set; }
        public ProjectReplaceData ProjectReplace { get; set; }
    }
}
