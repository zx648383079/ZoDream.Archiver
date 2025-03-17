using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ZoDream.ChmExtractor.Models;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.ChmExtractor
{
    public partial class ChmReader : IArchiveReader
    {
        public ChmReader(BinaryReader reader, IArchiveOptions options)
        {
            _reader = reader;
            _options = options;
            _basePosition = _reader.BaseStream.Position;
            Initialize();
        }

        private readonly BinaryReader _reader;
        private readonly IArchiveOptions _options;
        private readonly long _basePosition;
        private readonly ChmFile _header = new();

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            if (entry is not FileArchiveEntry item)
            {
                return;
            }
            DecompressRegion(new ChmUnitInfo()
            {
                Start = item.Offset,
                Length = item.Length,
                Space = item.IsCompressed ? 1 : 0,
            }, output);
        }

      

        private void ExtractTo(IReadOnlyEntry entry, string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, mode, out var fullPath))
            {
                return;
            }
            using var fs = File.Create(fullPath);
            ExtractTo(entry, fs);
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            var entries = ReadEntry();
            var i = 0;
            foreach (var item in entries)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                var fileName = Path.Combine(folder, item.Name);
                ExtractTo(item, fileName, mode);
                progressFn?.Invoke((double)(++i) / entries.Count());
            }
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            return ReadHeaderSectionTableEntry()
                .Where(i => i.Length > 0);
            // var pos = _reader.ReadUInt64();
            // _reader.BaseStream.Seek((long)pos, SeekOrigin.Begin);
            // ReadNameListFile();
            // ReadControlData();
        }

        public void Dispose()
        {
            if (_options?.LeaveStreamOpen == false)
            {
                _reader.BaseStream.Dispose();
            }
        }

    }
}
