using ZoDream.WallpaperExtractor.Models;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

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
            if (entry is not PackageEntry e)
            {
                return;
            }
            reader.BaseStream.Seek(e.Offset, SeekOrigin.Begin);
            reader.BaseStream.CopyTo(output, e.Length);
        }

        public void ExtractToDirectory(string folder, Action<double>? progressFn = null, CancellationToken token = default)
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
                    ExtractTo(item, fileName);
                }
                else
                {
                    new TexReader(
                        new BinaryReader(ConvertTo((PackageEntry)item))
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
            var entries = new PackageEntry[count];
            for (var i = 0; i < count; i++)
            {
                entries[i] = new PackageEntry(
                    reader.ReadNString(),
                    reader.ReadInt32(),
                    reader.ReadInt32()
                    );
            }
            var begin = reader.BaseStream.Position;
            foreach (var item in entries)
            {
                item.Offset += begin;
            }
            return entries;
        }

        private void ExtractTo(IReadOnlyEntry entry, string fileName)
        {
            PackageExtension.CreateDirectory(fileName);
            using var fs = File.Create(fileName);
            ExtractTo(entry, fs);
        }

        private PartialStream ConvertTo(PackageEntry entry)
        {
            return new PartialStream(reader.BaseStream, entry.Offset, entry.Length);
        }
    }
}
