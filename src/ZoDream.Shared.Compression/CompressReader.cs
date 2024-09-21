using SharpCompress.Readers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Compression
{
    public class CompressReader : IArchiveReader
    {
        public CompressReader(Stream stream, IArchiveOptions options)
        {
            _reader = ReaderFactory.Open(stream, CompressHelper.Convert(options));
        }

        public CompressReader(IReader reader)
        {
            _reader = reader;
        }

        private readonly IReader _reader;

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            while (_reader.MoveToNextEntry())
            {
                if (_reader.Entry.IsDirectory)
                {
                    continue;
                }
                if (_reader.Entry.Key == entry.Name)
                {
                    _reader.WriteEntryTo(output);
                    break;
                }
            }
        }

        public void ExtractToDirectory(string folder,
            ArchiveExtractMode mode,
            Action<double>? progressFn = null, CancellationToken token = default)
        {
            var i = 0D;
            while (_reader.MoveToNextEntry())
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                if (_reader.Entry.IsDirectory)
                {
                    continue;
                }
                _reader.WriteEntryToDirectory(folder, new() { Overwrite = mode == ArchiveExtractMode.Overwrite});
                progressFn?.Invoke(i += .1);
            }
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            while (_reader.MoveToNextEntry())
            {
                if (_reader.Entry.IsDirectory)
                {
                    continue;
                }
                yield return CompressHelper.Convert(_reader.Entry);
            }
        }

        public void Dispose()
        {
            _reader.Dispose();
        }
    }
}
