using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            return OwnArchiveReader.IsSupport(stream);
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            if (!IsReadable(stream))
            {
                return null;
            }
            return new OwnArchiveReader(stream, options);
        }

        public static IOwnKey CreateKey(IArchiveOptions options)
        {
            if (options?.Dictionary is null)
            {
                throw new ArgumentNullException("dictionary Path is must");
            }
            var bin = new OwnDictionary(File.OpenRead(options.Dictionary));
            if (string.IsNullOrWhiteSpace(options.Password))
            {
                return bin;
            }
            return new OwnMultipleKey(bin, new OwnPassword(options.Password));
        }
    }
}
