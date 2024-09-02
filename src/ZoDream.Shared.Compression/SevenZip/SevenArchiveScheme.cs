using SharpCompress.Archives.SevenZip;
using System;
using System.IO;
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
    }
}
