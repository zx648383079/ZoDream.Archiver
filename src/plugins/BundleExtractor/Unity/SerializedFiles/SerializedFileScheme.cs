using System;
using System.IO;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.SerializedFiles
{
    public class SerializedFileScheme : IArchiveScheme
    {
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public bool IsReadable(Stream stream)
        {
            return IsReadable(new EndianReader(stream, EndianType.BigEndian));
        }



        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            var r = new EndianReader(stream, EndianType.BigEndian);
            if (!IsReadable(r))
            {
                return null;
            }
            return new SerializedFileReader(r, filePath, options);
        }

        private static bool IsReadable(EndianReader reader)
        {
            var initialPosition = reader.BaseStream.Position;
            if (reader.BaseStream.Position + SerializedFileHeader.HeaderMinSize > reader.BaseStream.Length)
            {
                return false;
            }
            var metadataSize = reader.ReadUInt32();
            long headerDefinedFileSize = reader.ReadUInt32();
            var version = reader.ReadUInt32();
            long dataOffset = reader.ReadUInt32();
            if (!Enum.IsDefined(typeof(FormatVersion), (int)version))
            {
                reader.BaseStream.Position = initialPosition;
                return false;
            }
            if (version >= 22)
            {
                reader.BaseStream.Position = initialPosition + 0x14;
                metadataSize = reader.ReadUInt32();
                headerDefinedFileSize = reader.ReadInt64();
                dataOffset = reader.ReadInt64();
            }

            reader.BaseStream.Position = initialPosition;
            return metadataSize >= SerializedFileHeader.MetadataMinSize
                && headerDefinedFileSize >= SerializedFileHeader.HeaderMinSize + SerializedFileHeader.MetadataMinSize
                && reader.BaseStream.Length >= 0
                && headerDefinedFileSize == reader.BaseStream.Length;
        }
    }
}
