using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using ZoDream.Shared.Compression.Zip;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.EpubExtractor
{
    public class EpubReader(Stream stream, IArchiveOptions options) : IArchiveReader
    {
        private readonly ZipArchiveReader _reader = new(stream, options);

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            _reader.ExtractTo(new ReadOnlyEntry("OEBPS/" + entry.Name), output);
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            _reader.ExtractToDirectory(folder, mode, progressFn, token);
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            foreach (var item in _reader.ReadEntry())
            {
                if (string.IsNullOrWhiteSpace(item.Name) || !item.Name.StartsWith("OEBPS/"))
                {
                    continue;
                }
                var name = item.Name[6..];
                if (name == "content.opf" || name == "toc.ncx")
                {
                    continue;
                }
                yield return new ReadOnlyEntry(name,
                    item.Length,
                    item.CompressedLength, item.IsEncrypted, item.CreatedTime);
            }
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
