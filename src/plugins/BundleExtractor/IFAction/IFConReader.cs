using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ZoDream.Shared;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.IFAction
{
    public class IFConReader(IBundleBinaryReader reader, IArchiveOptions? options) : IArchiveReader
    {
        public const string Signature = "iFFile";
        public IFConReader(Stream stream, IArchiveOptions? options) 
            : this(new BundleBinaryReader(stream, EndianType.LittleEndian), options)
        {
            
        }

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            if (entry is not ArchiveEntry o)
            {
                return;
            }
            ReadStream(o).CopyTo(output);
        }

        private void ExtractTo(ArchiveEntry entry, string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, mode, out var fullPath))
            {
                return;
            }
            using var fs = File.Create(fullPath);
            ExtractTo(entry, fs);
        }

        private Stream ReadStream(ArchiveEntry entry)
        {
            return new PartialStream(reader.BaseStream, entry.Offset, entry.Length);
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            var items = ReadEntry().ToArray();
            var i = 0;
            foreach (var item in items)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                ExtractTo((ArchiveEntry)item, Path.Combine(folder, item.Name), mode);
                progressFn?.Invoke(++i / (double)items.Length);
            }
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            var signature = reader.ReadString(6);
            Expectation.ThrowIfNotSignature(Signature, signature, "Not a valid iFCon file.");
            var indexSize = reader.ReadInt32();
            var endIndexPos = reader.Position + indexSize;
            var fileCount = reader.ReadInt32();
            var pos = reader.Position;
            while (pos < endIndexPos)
            {
                reader.Position = pos;
                var offset = reader.ReadInt32();
                var length = reader.ReadInt32();
                var name = reader.ReadString();
                pos = reader.Position;
                yield return new ArchiveEntry(name, endIndexPos + offset, length);
            }
        }

        public void Dispose()
        {
            reader.Dispose();
        }
    }
}
