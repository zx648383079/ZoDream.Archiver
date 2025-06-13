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

        public async Task SaveAsAsync(HttpResponseMessage response, 
            Stream output, RequestToken token)
        {
            using var input = await ReadAsStreamAsync(response);
            await CopyToWithMemoryAsync(input, output, token);
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

        /// <summary>
        /// 直接保存
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="token"></param>
        public static async Task CopyToAsync(Stream input,
            Stream output,
            RequestToken token)
        {
            await CopyToAsync(input, 0, output, token);
        }
        /// <summary>
        /// 直接保存
        /// </summary>
        /// <param name="input"></param>
        /// <param name="length">只读取指定长度的数据</param>
        /// <param name="output"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static async Task CopyToAsync(Stream input,
            long length,
            Stream output,
            RequestToken token)
        {
            var buffer = new byte[CHUNK_SIZE];
            var byteReceived = 0L;
            int size;
            while (!token.IsCancellationRequested)
            {
                // 检查暂停
                await token.WaitWhilePausedAsync();
                var maxLength = Math.Min(buffer.Length, (int)(length - byteReceived));
                size = input.Read(buffer, 0, maxLength);
                if (size == 0)
                {
                    break;
                }
                output.Write(buffer, 0, size);
                byteReceived += size;
                token.Emit(byteReceived);
                if (byteReceived >= length)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// 通过使用内存加速获取
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public static async Task CopyToWithMemoryAsync(Stream input,
            Stream output, RequestToken token)
        {
            var pipe = new Pipe();
            var writing = WritePipeAsync(input, pipe.Writer, token);
            var reading = ReadPipeAsync(pipe.Reader, output, token);
            await Task.WhenAll(writing, reading);
        }

        private static async Task WritePipeAsync(Stream input,
            PipeWriter writer, RequestToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var memory = writer.GetMemory(CHUNK_SIZE);
                int bytesRead = await input.ReadAsync(memory);
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

        private static async Task ReadPipeAsync(PipeReader reader,
            Stream output, RequestToken token)
        {
            var byteReceived = 0L;
            while (!token.IsCancellationRequested)
            {
                var result = await reader.ReadAsync();
                var buffer = result.Buffer;
                foreach (var segment in buffer)
                {
                    await output.WriteAsync(segment);
                    byteReceived += segment.Length;
                    token.Emit(byteReceived);
                }
                reader.AdvanceTo(buffer.End);
                if (result.IsCompleted)
                {
                    break;
                }
            }
            await reader.CompleteAsync();
        }
    }
}
