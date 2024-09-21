using SharpCompress.Archives;
using SharpCompress.Archives.Tar;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Compression.Tar
{
    public class TarArchiveReader : IArchiveReader
    {

        public TarArchiveReader(Stream stream, IArchiveOptions options)
        {
            _reader = TarArchive.Open(stream, CompressHelper.Convert(options));
        }

        private readonly TarArchive _reader;

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            var reader = _reader.ExtractAllEntries();
            while (reader.MoveToNextEntry())
            {
                if (reader.Entry.Key == entry.Name)
                {
                    reader.WriteEntryTo(output);
                    break;
                }
            }
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            _reader.ExtractToDirectory(folder, progressFn, token);
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            foreach (var item in _reader.Entries)
            {
                yield return CompressHelper.Convert(item);
            }
        }

        public void Dispose()
        {
            _reader.Dispose();
        }

    }
}
