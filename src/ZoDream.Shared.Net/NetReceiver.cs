using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace ZoDream.Shared.Net
{
    public class NetReceiver(
        INetService service,
        HttpResponseMessage response, 
        RequestContext request) : INetReceiver
    {
    

        public void Pause()
        {
        }

        public void Resume()
        {
        }

        public async Task StartAsync(RequestToken token = default)
        {
            var fileName = service.GetFileName(response);
            if (string.IsNullOrWhiteSpace(fileName))
            {
                // TODO
            }
            using var fs = File.Create(fileName);
            await service.SaveAsAsync(response, fs, token);
        }

        public void Stop()
        {
        }

        public void Dispose()
        {
        }
    }
}
