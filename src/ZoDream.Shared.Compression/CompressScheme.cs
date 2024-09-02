using SharpCompress.Readers;
using System;
using System.IO;
using ZoDream.Shared.Compression.Own;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Compression
{
    public class CompressScheme : IArchiveScheme
    {
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public bool IsReadable(Stream stream)
        {
            return true;
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            var reader = new OwnArchiveScheme().Open(stream, filePath, fileName, options);
            if (reader is not null)
            {
                return reader;
            }
            stream.Seek(0, SeekOrigin.Begin);
            var reader2 = ReaderFactory.Open(stream, CompressHelper.Convert(options));
            if (reader2 is not null)
            {
                return new CompressReader(reader2);
            }
            return null;
        }
    }
}
