using System;
using System.Threading.Tasks;

namespace ZoDream.Shared.Net
{
    public interface INetReceiver : IDisposable
    {
        public Task StartAsync(RequestToken token = default);

        public void Stop();

        public void Pause();

        public void Resume();
    }
}
