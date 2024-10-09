using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.WebFiles
{
    public class WebFileReader(EndianReader reader, IArchiveOptions? options) : IArchiveReader
    {
        private const string Signature = "UnityWebData1.0";
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
            if (entry is not WebFileEntry item)
            {
                return;
            }
            reader.BaseStream.Seek(_basePosition + item.Offset, SeekOrigin.Begin);
            reader.BaseStream.CopyTo(output, item.Length);
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            var items = ReadEntry().ToArray();
            var i = 0;
            foreach (var item in items)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                var fullPath = Path.Combine(folder, item.Name);
                using var fs = File.Create(fullPath);
                ExtractTo(item, fs);
                progressFn?.Invoke((double)(++i) / items.Length);
            }

        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            reader.BaseStream.Seek(_basePosition, SeekOrigin.Begin);
            var signature = reader.ReadStringZeroTerm();
            Debug.Assert(signature == Signature, $"Signature '{signature}' doesn't match to '{Signature}'");

            var headerLength = reader.ReadInt32(); //total size of the header including the signature and all the entries.
            while (reader.BaseStream.Position - _basePosition < headerLength)
            {
                yield return WebFileEntry.Read(reader);
            }
        }

        public bool IsSupport()
        {
            return reader.ReadStringZeroTerm() == Signature;
        }
    }
}
