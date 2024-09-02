using SharpCompress.Archives.Tar;
using System.IO;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Compression.Tar
{
    public class TarArchiveScheme : IArchiveScheme
    {
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            return new TarArchiveWriter(stream, options);
        }

        public bool IsReadable(Stream stream)
        {
            return TarArchive.IsTarFile(stream);
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            if (!TarArchive.IsTarFile(stream))
            {
                return null;
            }
            return new TarArchiveReader(stream, options);
        }
    }
}
