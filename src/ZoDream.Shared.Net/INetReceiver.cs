using System;
using System.Threading.Tasks;
using ZoDream.Shared.Bundle;

namespace ZoDream.Shared.Net
{
    public interface INetReceiver : IDisposable
    {
        public Task StartAsync(IBundleToken token);

        public void Stop();

        public void Pause();

        public void Resume();
    }
}
