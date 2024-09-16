using System;
using System.Threading;

namespace ZoDream.Shared.Interfaces
{
    public interface IBundleReader : IDisposable
    {

        public void ExtractTo(string folder, CancellationToken token = default);
    }
}
