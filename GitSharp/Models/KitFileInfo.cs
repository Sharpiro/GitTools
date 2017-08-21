using System.IO;

namespace GitSharp.Models
{
    public class KitFileInfo
    {
        public FileInfo FileInfo { get; set; }
        public string Hash { get; set; }
        public string RelativePath { get; set; }
    }
}