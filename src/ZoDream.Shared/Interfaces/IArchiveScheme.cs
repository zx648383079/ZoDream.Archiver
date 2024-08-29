using System.IO;

namespace ZoDream.Shared.Interfaces
{
    public interface IArchiveScheme
    {
        public bool IsReadable(Stream stream);
        public IArchiveReader? Read(Stream stream, string filePath, string fileName);
    }
}
