using System;
using System.Threading;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Interfaces
{
    public interface IBundleReader : IDisposable
    {

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default);
    }
}
