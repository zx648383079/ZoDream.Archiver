using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.WallpaperExtractor
{
    public class PackageReader(BinaryReader reader, IArchiveOptions? options) : IArchiveReader
    {
        private readonly long _basePosition = reader.BaseStream.Position;
        public void Dispose()
        {
            if (options?.LeaveStreamOpen == false)
            {
                reader.Dispose();
            }
        }

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            if (entry is not ArchiveEntry e)
            {
                return;
            }
            reader.BaseStream.Seek(e.Offset, SeekOrigin.Begin);
            reader.BaseStream.CopyTo(output, e.Length);
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
                if (!item.Name.EndsWith(".tex"))
                {
                    ExtractTo(item, fileName, mode);
                }
                else
                {
                    new TexReader(
                        new BinaryReader(ConvertTo((ArchiveEntry)item))
                        , item.Name, null)
                        .ExtractTo(fileName);
                }
                progressFn?.Invoke((double)(++i) / entries.Count());
            }
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            reader.BaseStream.Seek(_basePosition, SeekOrigin.Begin);
            reader.ReadNString();
            var count = reader.ReadInt32();
            var entries = new ArchiveEntry[count];
            for (var i = 0; i < count; i++)
            {
                entries[i] = new ArchiveEntry(
                    reader.ReadNString(),
                    reader.ReadInt32(),
                    reader.ReadInt32()
                    );
            }
            var begin = reader.BaseStream.Position;
            foreach (var item in entries)
            {
                item.Move(begin);
            }
            return entries;
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

        private PartialStream ConvertTo(ArchiveEntry entry)
        {
            return new PartialStream(reader.BaseStream, entry.Offset, entry.Length);
        }
    }
}
