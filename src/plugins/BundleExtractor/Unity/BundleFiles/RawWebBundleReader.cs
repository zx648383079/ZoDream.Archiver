using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.BundleFiles
{
    public class RawWebBundleReader<THeader> : IArchiveReader
        where THeader : RawWebBundleHeader, new()
    {

        public RawWebBundleReader(IBundleBinaryReader reader, IArchiveOptions? options)
        {

            _options = options;
            _basePosition = reader.BaseStream.Position;
            _header.Read(reader);
            _headerLength = reader.BaseStream.Position - _basePosition;
            _reader = new BundleBinaryReader(ReadRawStream(reader.BaseStream), EndianType.BigEndian, reader);
        }

        private readonly THeader _header = new();
        private readonly IBundleBinaryReader _reader;
        private readonly IArchiveOptions? _options;
        private readonly long _basePosition;
        private readonly long _headerLength;
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
            if (entry is not RawWebEntry raw)
            {
                return;
            }
            using var ms = new PartialStream(_reader.BaseStream,
                _dataBeginPosition + raw.Offset, raw.Length);
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

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            //long metadataPosition = stream.Position;
            var items = _reader.ReadArray(RawWebEntry.Read);
            _reader.AlignStream();
            _dataBeginPosition = _reader.BaseStream.Position;
            return items;
            //if (metadataSize > 0)
            //{
            //    if (stream.Position - metadataPosition != metadataSize)
            //    {
            //        throw new Exception($"Read {stream.Position - metadataPosition} but expected {metadataSize} while reading bundle metadata");
            //    }
            //}
        }


        private Stream ReadRawStream(Stream input)
        {
            int metadataSize = RawWebBundleHeader.HasUncompressedBlocksInfoSize(
                _header.Version) ? _header.UncompressedBlocksInfoSize : 0;

            //These branches are collapsed by JIT
            if (typeof(THeader) == typeof(RawBundleHeader))
            {
                return new PartialStream(input, metadataSize);
            }
            else if (typeof(THeader) == typeof(WebBundleHeader))
            {
                // read only last chunk
                BundleScene chunkInfo = _header.Scenes[^1];
                var stream = BundleCodec.Decode(input, Models.CompressionType.Lzma, chunkInfo.CompressedSize, chunkInfo.DecompressedSize);
                try
                {
                    _ = stream.Position;
                    return stream;
                }
                catch (Exception)
                {
                    var ms = new MemoryStream();
                    stream.CopyTo(ms);
                    stream.Dispose();
                    return ms;
                }
            }
            else
            {
                throw new Exception($"Unsupported bundle type '{typeof(THeader)}'");
            }
        }

    }
}
