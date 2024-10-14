using ZoDream.Shared.Models;

namespace ZoDream.ChmExtractor
{
    public class FileArchiveEntry(string name, long offset, long length) : 
        ArchiveEntry(name, offset, length)
    {

        public int WindowSize { get; set; }
    }
}
