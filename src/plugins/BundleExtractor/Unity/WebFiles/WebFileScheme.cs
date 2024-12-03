using System;
using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.WebFiles
{
    public class WebFileScheme : IArchiveScheme
    {
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public bool IsReadable(Stream stream)
        {
            throw new NotImplementedException();
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            var reader = new WebFileReader(new BundleBinaryReader(stream, EndianType.LittleEndian), options);
            if (!reader.IsSupport())
            {
                return null;
            }
            return reader;
        }
    }
}
