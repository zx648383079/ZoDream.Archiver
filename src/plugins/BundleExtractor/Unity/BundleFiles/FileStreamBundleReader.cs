﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.BundleFiles
{
    public class FileStreamBundleReader : IArchiveReader
    {
        public FileStreamBundleReader(Stream stream, IArchiveOptions? options) : this(new BundleBinaryReader(stream, EndianType.BigEndian), options)
        {

        }

        public FileStreamBundleReader(IBundleBinaryReader reader, IArchiveOptions? options)
        {
            _reader = reader;
            _options = options;
            _basePosition = reader.Position;
            _header.Read(reader);
            var codec = reader.Get<IBundleCodec>();
            codec.Initialize(reader);
            _headerLength = reader.Position - _basePosition;
            _storageItems = new(codec);
        }

        private readonly IBundleBinaryReader _reader;
        private readonly IArchiveOptions? _options;
        private readonly long _basePosition;
        private readonly long _headerLength;
        private readonly FileStreamBundleHeader _header = new();
        private SplitStreamCollection _storageItems;
        private long _dataBeginPosition;

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            if (entry is not FileStreamEntry ery)
            {
                return;
            }
            _reader.BaseStream.Position = _dataBeginPosition;
            using var ms = _storageItems.Create(_reader.BaseStream,
                ery.Offset, ery.Length);
            ms.CopyTo(output);
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            var entries = ReadEntry();
            var i = 0;
            foreach (var item in entries)
            {
                using var fs = File.Create(Path.Combine(folder, item.Name));
                ExtractTo(item, fs);
                progressFn?.Invoke((double)(++i) / entries.Count());
            }
        }

        public void Dispose()
        {
            if (_options?.LeaveStreamOpen == false)
            {
                _reader.Dispose();
            }
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            if (_header.Version >= UnityBundleVersion.BF_LargeFilesSupport)
            {
                _reader.AlignStream(16);
            }
            if ((_header.Flags & BundleFlags.BlocksInfoAtTheEnd) != 0)
            {
                _reader.BaseStream.Seek(_basePosition + _header.Size - _header.CompressedBlocksInfoSize, SeekOrigin.Begin);
            }

            var metaCompression = (UnityCompressionType)(_header.Flags & BundleFlags.CompressionTypeMask);
            using var reader = _reader.Get<IBundleCodec>().Decode(
                _reader, metaCompression.ToCodec(),
                _header.CompressedBlocksInfoSize,
                _header.UncompressedBlocksInfoSize);
            var metadataPosition = reader.BaseStream.Position;
            var uncompressedDataHash = reader.ReadBytes(16);
            
            _storageItems = new SplitStreamCollection(reader.Get<IBundleCodec>(), reader.ReadArray(StorageEntry.Read));
            var items = Array.Empty<FileStreamEntry>();
            if ((_header.Flags & BundleFlags.BlocksAndDirectoryInfoCombined) != 0)
            {
                items = reader.ReadArray(FileStreamEntry.Read);
            }
            if (_header.UncompressedBlocksInfoSize > 0)
            {
                if (reader.BaseStream.Position - metadataPosition != _header.UncompressedBlocksInfoSize)
                {
                    throw new Exception($"Read {reader.BaseStream.Position - metadataPosition} but expected {_header.UncompressedBlocksInfoSize} while reading bundle metadata");
                }
            }

            if ((_header.Flags & BundleFlags.BlocksInfoAtTheEnd) != 0)
            {
                _reader.BaseStream.Seek(_basePosition + _headerLength, SeekOrigin.Begin);
                if (_header.Version >= UnityBundleVersion.BF_LargeFilesSupport)
                {
                    _reader.AlignStream(16);
                }
            }
            if ((_header.Flags & BundleFlags.BlockInfoNeedPaddingAtStart) != 0)
            {
                _reader.AlignStream(16);
            }
            _dataBeginPosition = _reader.BaseStream.Position;
            return items;
        }

    }
}
