using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public class MiHoYoBundleReader : IArchiveReader
    {
        public MiHoYoBundleReader(Stream stream, IArchiveOptions? option)
            : this(new BundleBinaryReader(stream, EndianType.LittleEndian), option)
        {

        }
        public MiHoYoBundleReader(IBundleBinaryReader reader, IArchiveOptions? options)
        {
            _storageItems = new(reader.Get<IBundleCodec>());
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
        private readonly MiHoYoBundleHeader _header = new();
        private readonly SplitStreamCollection _storageItems;

        public void Dispose()
        {
            if (_options?.LeaveStreamOpen == false)
            {
                _reader.Dispose();
            }
        }

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            throw new NotImplementedException();
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            throw new NotImplementedException();
        }

        public int ReadMhyInt()
        {
            var buffer = _reader.ReadBytes(6);
            return buffer[2] | buffer[4] << 8 | buffer[0] << 0x10 | buffer[5] << 0x18;
        }

        public uint ReadMhyUInt()
        {
            var buffer = _reader.ReadBytes(7);
            return (uint)(buffer[1] | buffer[6] << 8 | buffer[3] << 0x10 | buffer[2] << 0x18);
        }

        public string ReadMhyString()
        {
            var pos = _reader.BaseStream.Position;
            var str = _reader.ReadStringZeroTerm();
            _reader.BaseStream.Position += 0x105 - (_reader.BaseStream.Position - pos);
            return str;
        }
    }
}
