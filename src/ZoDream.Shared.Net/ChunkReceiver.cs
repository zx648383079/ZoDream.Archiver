using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ZoDream.Shared.Net
{
    public class ChunkReceiver : INetReceiver
    {
        public ChunkReceiver(INetService service, HttpResponseMessage res, RequestContext request)
        {
        }

        public void Pause()
        {
        }

        public void Resume()
        {
        }

        public Task StartAsync(CancellationToken token = default)
        {
        }

        public void Stop()
        {
        }

        public void Dispose()
        {
        }
    }
}
