using System.IO;

namespace ZoDream.Shared.Interfaces
{
    public interface IArchiveScheme
    {
        public bool IsReadable(Stream stream);
        public IArchiveReader? Open(Stream stream, 
            string filePath, 
            string fileName, IArchiveOptions? options = null);

        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null);
    }
}
