using System;
using System.Threading;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleHandler : IDisposable
    {

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default);
    }
}
