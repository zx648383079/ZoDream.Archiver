using SharpCompress.Archives.Tar;
using System.IO;
using System.Threading.Tasks;
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
