using System;
using System.IO;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Compression.Own
{
    public class OwnArchiveScheme : IArchiveScheme
    {
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            return new OwnArchiveWriter(stream, options);
        }

        public bool IsReadable(Stream stream)
        {
            return IsSupport(stream) is not null;
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            var header = IsSupport(stream);
            if (header is null)
            {
                return null;
            }
            if (header.Version == OwnVersion.V2)
            {
                return new V2.OwnArchiveReader(stream, options);
            }
            return new OwnArchiveReader(stream, options);
        }

        public static IOwnKey CreateKey(IArchiveOptions options)
        {
            if (options?.Dictionary is null)
            {
                throw new CryptographicException("dictionary Path is must");
            }
            var bin = new OwnDictionary(File.OpenRead(options.Dictionary));
            if (string.IsNullOrWhiteSpace(options.Password))
            {
                return bin;
            }
            return new OwnMultipleKey(bin, new OwnPassword(options.Password));
        }

        public static OwnFileHeader? IsSupport(Stream stream)
        {
            stream.Seek(0, SeekOrigin.Begin);
            try
            {
                var header = new OwnFileHeader();
                header.Read(stream);
                return header;
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                stream.Seek(0, SeekOrigin.Begin);
            }
        }
    }
}
