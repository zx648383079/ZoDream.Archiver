using SharpCompress.Archives.Zip;
using System.IO;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Compression.Zip
{
    public class ZipArchiveScheme : IArchiveScheme
    {
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            return new ZipArchiveWriter(stream, options);
        }

        public bool IsReadable(Stream stream)
        {
            return ZipArchive.IsZipFile(stream);
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            if (!ZipArchive.IsZipFile(stream, options?.Password))
            {
                return null;
            }
            return new ZipArchiveReader(stream, options);
        }
    }
}
