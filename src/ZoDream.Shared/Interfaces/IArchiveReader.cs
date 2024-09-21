using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Interfaces
{
    public interface IArchiveReader : IDisposable
    {

        public IEnumerable<IReadOnlyEntry> ReadEntry();

        public void ExtractTo(IReadOnlyEntry entry, Stream output);
        public void ExtractToDirectory(string folder,
            ArchiveExtractMode mode,
            Action<double>? progressFn = null, 
            CancellationToken token = default);
    }
}
