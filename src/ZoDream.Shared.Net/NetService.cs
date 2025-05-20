using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.IO.Pipelines;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ZoDream.Shared.Net
{
    public class NetService : INetService
    {
        const int CHUNK_SIZE = 8192;

        private readonly TimeSpan _timeout = TimeSpan.FromSeconds(600);

        public CookieContainer Cookie { get; private set; } = new();

        public IWebProxy? Proxy { get; private set; }

        public IDictionary<string, string> Headers { get; private set; } = new ConcurrentDictionary<string, string>();

        public async Task<HttpResponseMessage> GetAsync(Uri url, CancellationToken token = default)
        {
            using var request = PrepareRequest();
            using var client = PrepareClient();
            request.Method = HttpMethod.Get;
            request.RequestUri = url;
            return await client.SendAsync(request, token);
        }

        public async Task<HttpResponseMessage> PostAsync(Uri url, HttpContent body, CancellationToken token = default)
        {
            using var request = PrepareRequest();
            using var client = PrepareClient();
            request.Method = HttpMethod.Post;
            request.RequestUri = url;
            request.Content = body;
            return await client.SendAsync(request, token);
        }

        public async Task<HttpResponseMessage> DeleteAsync(Uri url, CancellationToken token = default)
        {
            using var request = PrepareRequest();
            using var client = PrepareClient();
            request.Method = HttpMethod.Delete;
            request.RequestUri = url;
            return await client.SendAsync(request, token);
        }

        public async Task<HttpResponseMessage> SendAsync(RequestContext request)
        {
            using var message = PrepareRequest();
            using var client = PrepareClient();
            message.Method = request.Method;
            message.RequestUri = request.Source;
            message.Content = request.Body;
            return await client.SendAsync(message, request.Token.Cancellation);
        }

        public async Task<HttpResponseMessage> SendAsync(RequestContext request, RangeHeaderValue range)
        {
            using var message = PrepareRequest();
            using var client = PrepareClient();
            message.Method = request.Method;
            message.RequestUri = request.Source;
            message.Content = request.Body;
            message.Headers.Range = range;
            return await client.SendAsync(message, request.Token.Cancellation);
        }

        public async Task<Stream> ReadAsStreamAsync(HttpResponseMessage response)
        {
            return await response.Content.ReadAsStreamAsync();
        }

        public async Task SaveAsAsync(HttpResponseMessage response, Stream output, RequestToken token)
        {
            using var input = await ReadAsStreamAsync(response);
            await CopyToWithMemoryAsync(output, input, token);
        }

        public long GetContentLength(HttpResponseMessage response)
        {
            return response.Content.Headers.ContentLength ?? 0;
        }
        public string GetFileName(HttpResponseMessage response)
        {
            return response.Content.Headers.ContentDisposition?.FileName ?? string.Empty;
        }
        public bool GetAcceptRange(HttpResponseMessage response)
        {
            return response.Headers.AcceptRanges?.Count > 0;
        }

        /// <summary>
        /// 直接保存
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="token"></param>
        private static async Task CopyToAsync(Stream input, Stream output, RequestToken token)
        {
            var buffer = new byte[CHUNK_SIZE];
            var byteReceived = 0L;
            int size;
            while (true)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                // 检查暂停
                await token.WaitWhilePausedAsync();
                size = input.Read(buffer, 0, buffer.Length);
                if (size == 0)
                {
                    break;
                }
                output.Write(buffer, 0, size);
                byteReceived += size;
            }
        }

        /// <summary>
        /// 通过使用内存加速获取
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        private static async Task CopyToWithMemoryAsync(Stream input, Stream output, RequestToken token)
        {
            var pipe = new Pipe();
            var writing = WritePipeAsync(input, pipe.Writer);
            var reading = ReadPipeAsync(pipe.Reader, output);
            await Task.WhenAll(writing, reading);
        }

        private static async Task WritePipeAsync(Stream stream, PipeWriter writer)
        {
            while (true)
            {
                var memory = writer.GetMemory(CHUNK_SIZE);
                int bytesRead = await stream.ReadAsync(memory);
                if (bytesRead == 0)
                {
                    break;
                }
                writer.Advance(bytesRead);
                var result = await writer.FlushAsync();
                if (result.IsCompleted)
                {
                    break;
                }
            }
            await writer.CompleteAsync();
        }

        private static async Task ReadPipeAsync(PipeReader reader, Stream fileStream)
        {
            while (true)
            {
                var result = await reader.ReadAsync();
                var buffer = result.Buffer;
                foreach (var segment in buffer)
                {
                    await fileStream.WriteAsync(segment);
                }
                reader.AdvanceTo(buffer.End);
                if (result.IsCompleted)
                {
                    break;
                }
            }
            await reader.CompleteAsync();
        }

        private HttpMessageHandler PrepareHandler()
        {
            var handler = new SocketsHttpHandler()
            {
                AutomaticDecompression = DecompressionMethods.All,
                AllowAutoRedirect = true,
                UseCookies = true,
                CookieContainer = Cookie,
                Proxy = Proxy,
                ConnectTimeout = _timeout,
            };
            return handler;
        }

        private HttpClient PrepareClient()
        {
            return new HttpClient(PrepareHandler());
        }

        private HttpRequestMessage PrepareRequest()
        {
            var request = new HttpRequestMessage();
            foreach (var item in Headers)
            {
                request.Headers.TryAddWithoutValidation(item.Key, item.Value);
            }
            return request;
        }

        public void Dispose()
        {
        }
    }
}
