using System.IO;

namespace ZoDream.Shared.Interfaces
{
    public interface IArchiveScheme
    {
        public IArchiveReader? Open(Stream stream, 
            string filePath, 
            string fileName, IArchiveOptions? options = null);

        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null);
    }
}
