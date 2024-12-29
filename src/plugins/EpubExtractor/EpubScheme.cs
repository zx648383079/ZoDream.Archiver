using SharpCompress.Archives.Zip;
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
            return ZipArchive.IsZipFile(stream);
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            if (!fileName.EndsWith(".epub") || !IsReadable(stream))
            {
                return null;
            }
            return new EpubReader(stream, options);
        }
    }
}
