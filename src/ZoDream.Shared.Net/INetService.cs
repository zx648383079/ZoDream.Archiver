using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ZoDream.Shared.Net
{
    public interface INetService
    {

        public CookieContainer Cookie { get; }

        public IWebProxy? Proxy { get; }

        public IDictionary<string, string> Headers { get; }

        public Task<HttpResponseMessage> GetAsync(Uri url, CancellationToken token = default);
        public Task<HttpResponseMessage> PostAsync(Uri url, HttpContent body, CancellationToken token = default);
        public Task<HttpResponseMessage> DeleteAsync(Uri url, CancellationToken token = default);

        public Task<Stream?> ReadStreamAsync(HttpResponseMessage response);

    }
}
