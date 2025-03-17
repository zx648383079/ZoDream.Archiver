using ZoDream.Shared.Models;

namespace ZoDream.ChmExtractor
{
    public class FileArchiveEntry(string name, long offset, long length, bool isCompressed) : 
        ArchiveEntry(name, offset, length)
    {

        public bool IsCompressed => isCompressed;
    }
}
