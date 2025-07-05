using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using ZoDream.Shared.Bundle;

namespace ZoDream.Shared.Net
{
    public class NetReceiver(
        INetService service,
        HttpResponseMessage response, Stream output) : INetReceiver
    {
    

        public void Pause()
        {
        }

        public void Resume()
        {
        }

        public async Task StartAsync(IBundleToken token)
        {
            await service.SaveAsAsync(response, output, token);
        }

        public void Stop()
        {
        }

        public void Dispose()
        {
        }
    }
}
