﻿using System;
using System.IO;
using System.Threading.Tasks;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.SerializedFiles
{
    public class SerializedFileScheme : IArchiveScheme, IBundleArchiveScheme
    {
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public bool IsReadable(Stream stream)
        {
            return IsReadable(new BundleBinaryReader(stream, EndianType.BigEndian));
        }

        public Task<IArchiveReader?> OpenAsync(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            return Task.FromResult(Open(stream, filePath, fileName, options));
        }

        public Task<IArchiveWriter> CreateAsync(Stream stream, IArchiveOptions? options = null)
        {
            return Task.FromResult(Create(stream, options));
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            var r = new BundleBinaryReader(stream, EndianType.BigEndian);
            if (!IsReadable(r))
            {
                return null;
            }
            return new SerializedFileReader(r, FilePath.Parse(filePath), options);
        }

        private static bool IsReadable(IBundleBinaryReader reader)
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

        public IArchiveReader? Open(IBundleBinaryReader reader, IFilePath sourcePath, IArchiveOptions? options = null)
        {
            if (!IsReadable(reader))
            {
                return null;
            }
            return new SerializedFileReader(reader, sourcePath, options);
        }
    }
}
