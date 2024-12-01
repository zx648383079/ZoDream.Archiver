using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.BundleExtractor.Models;
using ZoDream.BundleExtractor.Unity.BundleFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public class BlbBundleReader : IArchiveReader
    {
        public BlbBundleReader(Stream stream, IArchiveOptions? option)
            : this(new BundleBinaryReader(stream, EndianType.LittleEndian), option)
        {

        }
        public BlbBundleReader(IBundleBinaryReader reader, IArchiveOptions? options)
        {
            _reader = reader;
            _options = options;
            _basePosition = reader.BaseStream.Position;
            _header.Read(reader);
            _headerLength = reader.BaseStream.Position - _basePosition;
        }

        private readonly IBundleBinaryReader _reader;
        private readonly IArchiveOptions? _options;
        private readonly long _basePosition;
        private readonly long _headerLength;
        private readonly BlbBundleHeader _header = new();
        private readonly SplitStreamCollection _storageItems = [];
        private long _dataBeginPosition;
        public void Dispose()
        {
            if (_options?.LeaveStreamOpen == false)
            {
                _reader.Dispose();
            }
        }

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            if (entry is not FileStreamEntry ery)
            {
                return;
            }
            _reader.BaseStream.Position = _dataBeginPosition;
            using var ms = _storageItems.Create(_reader.BaseStream, ery.Offset, ery.Length);
            ms.CopyTo(output);
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            var entries = ReadEntry().ToArray();
            var i = 0;
            foreach (var item in entries)
            {
                using var fs = File.Create(Path.Combine(folder, item.Name));
                ExtractTo(item, fs);
                progressFn?.Invoke((double)(++i) / entries.Count());
            }
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            _header.Size = _reader.ReadUInt32();
            var lastUncompressedSize = _reader.ReadUInt32();

            _reader.BaseStream.Position += 4;
            var blobOffset = _reader.ReadInt32();
            var blobSize = _reader.ReadUInt32();
            var compressionType = (CompressionType)_reader.ReadByte();
            var uncompressedSize = (uint)1 << _reader.ReadByte();
            _reader.AlignStream();

            var blocksInfoCount = _reader.ReadInt32();
            var nodesCount = _reader.ReadInt32();

            var blocksInfoOffset = _reader.BaseStream.Position + _reader.ReadInt64();
            var nodesInfoOffset = _reader.BaseStream.Position + _reader.ReadInt64();
            var flagInfoOffset = _reader.BaseStream.Position + _reader.ReadInt64();

            for (var i = 0; i < blocksInfoCount; i++)
            {
                var compressedSize = _reader.ReadUInt32();

                _storageItems.Add(new(
                    i == blocksInfoCount - 1 ? lastUncompressedSize : uncompressedSize,
                    compressedSize,
                    compressionType)
                );
            }
            _reader.BaseStream.Position = nodesInfoOffset;
            var items = new FileStreamEntry[nodesCount];
            for (var i = 0; i < nodesCount; i++)
            {
                var offset = _reader.ReadInt32();
                var size = _reader.ReadInt32();
                var pos = _reader.BaseStream.Position;
                _reader.BaseStream.Position = flagInfoOffset;
                var flag = _reader.ReadUInt32();
                if (i >= 0x20)
                {
                    flag = _reader.ReadUInt32();
                }
                _reader.BaseStream.Position = pos;

                var pathOffset = _reader.BaseStream.Position + _reader.ReadInt64();

                pos = _reader.BaseStream.Position;
                _reader.BaseStream.Position = pathOffset;
                var path = _reader.ReadStringZeroTerm();
                _reader.BaseStream.Position = pos;
                items[i] = new FileStreamEntry(path, size)
                {
                    Offset = offset,
                    Flags = (NodeFlags)((flag & 1 << i) * 4)
                };
            }
            _dataBeginPosition = _reader.BaseStream.Position;
            return items;
        }
    }
}
