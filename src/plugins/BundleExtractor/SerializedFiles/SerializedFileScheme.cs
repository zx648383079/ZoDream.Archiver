using System;
using System.IO;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.SerializedFiles
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

        private bool IsReadable(EndianReader reader)
        {
            var initialPosition = reader.BaseStream.Position;
            if (reader.BaseStream.Position + SerializedFileHeader.HeaderMinSize > reader.BaseStream.Length)
            {
                return false;
            }
            int metadataSize = reader.ReadInt32();
            ulong headerDefinedFileSize = reader.ReadUInt32();
            var generation = reader.ReadInt32();
            if (!Enum.IsDefined(typeof(FormatVersion), generation))
            {
                reader.BaseStream.Position = initialPosition;
                return false;
            }
            if (generation >= 22)
            {
                reader.BaseStream.Position = initialPosition + 0x14;
                metadataSize = reader.ReadInt32();
                headerDefinedFileSize = reader.ReadUInt64();
            }

            reader.BaseStream.Position = initialPosition;

            return metadataSize >= SerializedFileHeader.MetadataMinSize
                && headerDefinedFileSize >= SerializedFileHeader.HeaderMinSize + SerializedFileHeader.MetadataMinSize
                && reader.BaseStream.Length >= 0
                && headerDefinedFileSize == (ulong)reader.BaseStream.Length;
        }
    }
}
