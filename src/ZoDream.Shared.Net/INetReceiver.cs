using System;
using System.Threading;
using System.Threading.Tasks;

namespace ZoDream.Shared.Net
{
    public interface INetReceiver : IDisposable
    {
        public Task StartAsync(CancellationToken token = default);

        public void Stop();

        public void Pause();

        public void Resume();
    }
}
