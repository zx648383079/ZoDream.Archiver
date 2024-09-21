using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.Shared.IO
{
    public class StreamArchiveReader(string fileName, Stream stream, IArchiveOptions? options) : IArchiveReader
    {
        

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            stream.CopyTo(output);
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            if (!LocationStorage.TryCreate(Path.Combine(folder, fileName), mode, out var fullPath))
            {
                return;
            }
            using var fs = File.Create(fullPath);
            stream.CopyTo(fs);
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            return [new ReadOnlyEntry(fileName, stream.Length)];
        }

        public void Dispose()
        {
            if (options?.LeaveStreamOpen == false)
            {
                stream.Dispose();
            }
        }
    }
}
