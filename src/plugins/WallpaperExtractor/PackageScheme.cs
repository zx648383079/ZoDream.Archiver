using System;
using System.IO;
using ZoDream.Shared.Interfaces;

namespace ZoDream.WallpaperExtractor
{
    public class PackageScheme : IArchiveScheme
    {
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public bool IsReadable(Stream stream)
        {
            var postion = stream.Position;
            var reader = new BinaryReader(stream);
            var res = reader.ReadNString().StartsWith("PKG");
            stream.Seek(postion, SeekOrigin.Begin);
            return res;
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            if (!fileName.EndsWith(".pkg") || !IsReadable(stream))
            {
                return null;
            }
            return new PackageReader(new BinaryReader(stream), options);
        }
    }
}
