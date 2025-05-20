using System;
using System.Net.Http;

namespace ZoDream.Shared.Net
{
    public struct RequestContext
    {
        public int SourceId;

        public Uri Source;

        public string OutputFolder;

        public HttpMethod Method;

        public HttpContent? Body;

        public RequestToken Token;
    }
}
