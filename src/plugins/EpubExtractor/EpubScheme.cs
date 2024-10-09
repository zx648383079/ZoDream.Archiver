using System.IO;
using ZoDream.Shared.Interfaces;

namespace ZoDream.EpubExtractor
{
    public class EpubScheme : IArchiveScheme
    {
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            return new EpubWriter(stream, options);
        }

        public bool IsReadable(Stream stream)
        {
            throw new System.NotImplementedException();
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            if (!IsReadable(stream))
            {
                return null;
            }
            return new EpubReader(stream, options);
        }
    }
}
