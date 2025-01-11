using SharpCompress.Archives.Rar;
using System;
using System.IO;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Compression.WinRar
{
    public class RarArchiveScheme : IArchiveScheme
    {
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public bool IsReadable(Stream stream)
        {
            return RarArchive.IsRarFile(stream);
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            if (!RarArchive.IsRarFile(stream))
            {
                return null;
            }
            return new RarArchiveReader(stream, options);
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
