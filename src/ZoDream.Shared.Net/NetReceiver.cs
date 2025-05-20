using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ZoDream.Shared.Net
{
    public class NetReceiver(HttpResponseMessage res, RequestContext request) : INetReceiver
    {

        public void Pause()
        {
            throw new NotImplementedException();
        }

        public void Resume()
        {
            throw new NotImplementedException();
        }

        public Task StartAsync(CancellationToken token = default)
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
