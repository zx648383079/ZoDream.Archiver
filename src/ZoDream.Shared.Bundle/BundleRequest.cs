using System;

namespace ZoDream.Shared.Bundle
{
    public class NetRequest(Uri url) : INetRequest
    {
        public Uri Source => url;

        public IBundleExecutor? Executor {  get; private set; }
    }
}
