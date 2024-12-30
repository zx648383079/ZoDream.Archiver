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
            var reader = new BinaryReader(stream);
            return IsPackage(reader);
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            var reader = new BinaryReader(stream);
            if (fileName.EndsWith(".pkg") && IsPackage(reader))
            {
                return new PackageReader(reader, options);
            }
            if (fileName.EndsWith(".tex") && IsTex(reader))
            {
                return new TexReader(reader, fileName, options);
            }
            return null;
        }

        private bool IsPackage(BinaryReader reader)
        {
            var position = reader.BaseStream.Position;
            try
            {
                return reader.ReadNString().StartsWith("PKG");
            }
            finally
            {
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
            }
        }

        private bool IsTex(BinaryReader reader)
        {
            var position = reader.BaseStream.Position;
            try
            {
                return reader.ReadNZeroString(16) == "TEXV0005";
            }
            finally
            {
                reader.BaseStream.Seek(position, SeekOrigin.Begin);
            }
        }
    }
}
