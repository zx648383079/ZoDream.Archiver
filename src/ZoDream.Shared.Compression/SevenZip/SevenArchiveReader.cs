using SharpCompress.Archives;
using SharpCompress.Archives.SevenZip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Compression.SevenZip
{
    public class SevenArchiveReader : IArchiveReader
    {

        public SevenArchiveReader(Stream stream, IArchiveOptions options)
        {
            _reader = SevenZipArchive.Open(stream, CompressHelper.Convert(options));
        }

        private readonly SevenZipArchive _reader;

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

        public void ExtractToDirectory(string folder, Action<double>? progressFn = null, CancellationToken token = default)
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
