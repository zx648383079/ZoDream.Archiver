using SharpCompress.Archives.Zip;
using System.IO;
using System.Threading.Tasks;
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

        public Task<IArchiveReader?> OpenAsync(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            return Task.FromResult(Open(stream, filePath, fileName, options));
        }

        public Task<IArchiveWriter> CreateAsync(Stream stream, IArchiveOptions? options = null)
        {
            return Task.FromResult(Create(stream, options));
        }
    }
}
