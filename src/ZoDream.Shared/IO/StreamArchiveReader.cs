using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.IO
{
    public class StreamArchiveReader(string fileName, Stream stream, IArchiveOptions? options) : IArchiveReader
    {
        

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            stream.CopyTo(output);
        }

        public void ExtractToDirectory(string folder, Action<double>? progressFn = null, CancellationToken token = default)
        {
            using var fs = File.Create(Path.Combine(folder, fileName));
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
