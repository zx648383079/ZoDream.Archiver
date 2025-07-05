using System;
using System.Net.Http;

namespace ZoDream.Shared.Bundle
{
    public class NetRequest(long requestId, Uri url, string outputFolder, IBundleToken token) : INetRequest
    {
        public NetRequest(IBundleRequest source, Uri url, string outputFolder)
            : this(source.RequestId, url, outputFolder, source.Token)
        {
            
        }
        public Uri Source => url;

        public string Output => outputFolder;

        public IBundleExecutor? Executor {  get; init; }

        public long RequestId => requestId;

        public IBundleToken Token => token;
    }

    public class NetPostRequest(long requestId, 
        Uri url, HttpContent body, string outputFolder, IBundleToken token) 
        : NetRequest(requestId, url, outputFolder, token), INetRequest
    {

        public NetPostRequest(IBundleRequest source, 
            Uri url, HttpContent body, string outputFolder)
            : this(source.RequestId, url, body, outputFolder, source.Token)
        {
            
        }

        public HttpContent Body => body;
    }
}
