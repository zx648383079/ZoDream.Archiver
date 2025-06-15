using System;
using System.Net.Http;

namespace ZoDream.Shared.Bundle
{
    public class NetRequest(Uri url, string outputFolder) : INetRequest
    {
        public Uri Source => url;

        public string Output => outputFolder;

        public IBundleExecutor? Executor {  get; private set; }

        
    }

    public class NetPostRequest(Uri url, HttpContent body, string outputFolder) : NetRequest(url, outputFolder), INetRequest
    {

        public HttpContent Body => body;
    }
}
