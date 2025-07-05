using System;
using System.Net.Http;
using ZoDream.Shared.Bundle;

namespace ZoDream.Shared.Net
{
    public interface IRequestContext
    {
        public Uri Source { get; }

        public HttpMethod Method { get; }

        public HttpContent? Body { get; }

        public IBundleToken Token { get; }
    }

    public struct RequestContext : INetRequest, IRequestContext
    {
        public long RequestId { get; init; }

        public Uri Source { get; init; }

        public string Output { get; init; }

        public IBundleExecutor? Executor { get; init; }

        public HttpMethod Method { get; init; }

        public HttpContent? Body { get; init; }

        public IBundleToken Token { get; init; }

        public string? SuggestedName { get; init; }
    }
}
