using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace ZoDream.Shared.Interfaces
{
    public interface IArchiveReader : IDisposable
    {

        public IEnumerable<IReadOnlyEntry> ReadEntry();

        public void ExtractTo(IReadOnlyEntry entry, Stream output);
        public void ExtractToDirectory(string folder, Action<double>? progressFn = null, CancellationToken token = default);
    }
}
