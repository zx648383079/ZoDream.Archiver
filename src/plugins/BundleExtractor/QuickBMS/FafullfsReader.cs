using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ZoDream.Shared;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.QuickBMS
{
    public class FafullfsReader(IBundleBinaryReader reader, IArchiveOptions options) 
        : IBundleHandler, IArchiveReader
    {
        private static readonly string Signature = "FAFULLFS";

        public void ExtractTo(string folder, 
            ArchiveExtractMode mode, 
            CancellationToken token = default)
        {
            var entries = ReadEntry();
            foreach (var item in entries)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                var fileName = Path.Combine(folder, item.Name);
                ExtractTo(item, fileName, mode);
            }
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            var buffer = reader.ReadBytes(Signature.Length);
            Expectation.ThrowIfNotSignature(buffer.Equal(Signature));
            var fileCount = reader.ReadLongLong();
            var namesOffset = reader.ReadLongLong();
            var namesLength = reader.ReadLongLong();
            var infoOffset = reader.ReadLongLong();
            reader.Position = infoOffset;
            for (int i = 0; i < fileCount; i++)
            {
                var crc = reader.ReadLongLong();
                var nameOffset = reader.ReadLongLong();
                reader.ReadLongLong();
                var size = reader.ReadLongLong();
                var offset = reader.ReadLongLong();
                var tstamp32 = reader.ReadLongLong();
                var pos = reader.Position;

                reader.Position = namesOffset + nameOffset;
                var name = reader.ReadStringZeroTerm();
                yield return new ArchiveEntry(name, offset, size);
                reader.Position = pos;
            }
        }

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            if (entry is not ArchiveEntry o)
            {
                return;
            }
            reader.Position = o.Offset;
            reader.ReadAsStream(o.Length).CopyTo(output);
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

        public void ExtractToDirectory(string folder, 
            ArchiveExtractMode mode, 
            Action<double>? progressFn = null, 
            CancellationToken token = default)
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

        public void Dispose()
        {
            if (options.LeaveStreamOpen)
            {
                return;
            }
            reader.Dispose();
        }


    }
}
