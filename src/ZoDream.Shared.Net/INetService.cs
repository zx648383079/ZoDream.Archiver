using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ZoDream.Shared.Net
{
    public interface INetService : IDisposable
    {

        public CookieContainer Cookie { get; }

        public IWebProxy? Proxy { get; }

        public IDictionary<string, string> Headers { get; }

        public Task<HttpResponseMessage> GetAsync(Uri url, CancellationToken token = default);
        public Task<HttpResponseMessage> PostAsync(Uri url, HttpContent body, CancellationToken token = default);
        public Task<HttpResponseMessage> DeleteAsync(Uri url, CancellationToken token = default);

        public Task<HttpResponseMessage> SendAsync(RequestContext request);
        public Task<HttpResponseMessage> SendAsync(RequestContext request, RangeHeaderValue range);

        public Task<Stream> ReadAsStreamAsync(HttpResponseMessage response);
        public Task SaveAsAsync(HttpResponseMessage response, Stream output, RequestToken token);


        public long GetContentLength(HttpResponseMessage response);
        public string GetFileName(HttpResponseMessage response);
        public bool GetAcceptRange(HttpResponseMessage response);
    }
}
