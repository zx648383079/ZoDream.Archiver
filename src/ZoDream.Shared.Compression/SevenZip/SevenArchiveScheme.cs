using SharpCompress.Archives.SevenZip;
using System;
using System.IO;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Compression.SevenZip
{
    public class SevenArchiveScheme : IArchiveScheme
    {
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public bool IsReadable(Stream stream)
        {
            return SevenZipArchive.IsSevenZipFile(stream);
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            if (!SevenZipArchive.IsSevenZipFile(stream))
            {
                return null;
            }
            return new SevenArchiveReader(stream, options);
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
