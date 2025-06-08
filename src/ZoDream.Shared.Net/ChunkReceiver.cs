using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ZoDream.Shared.Net
{
    public class ChunkReceiver(INetService service,
        HttpResponseMessage response,
        RequestContext request) : INetReceiver
    {
        private readonly SemaphoreSlim _semaphore = new(4);

        public void Pause()
        {
        }

        public void Resume()
        {
        }

        public Task StartAsync(RequestToken token = default)
        {
            var length = service.GetContentLength(response);
            var chunkSize = 1000000;
            var offset = 0;
            while (offset < length && token.IsCancellationRequested)
            {
                var currentOffset = offset;
                var currentLength = Math.Min(chunkSize, length - offset);
                ThreadPool.QueueUserWorkItem(async _ => {
                    await _semaphore.WaitAsync(token.Cancellation);
                    try
                    {
                        using var fs = File.Create($"_temp{offset}");
                        if (currentOffset == 0)
                        {
                            await service.SaveAsAsync(response, fs, token);
                        } else
                        {
                            using var chunk = await service.SendAsync(request,
                                new RangeHeaderValue(currentOffset, currentOffset + currentLength));
                            await service.SaveAsAsync(chunk, fs, token);
                        }
                    }
                    finally
                    {
                        _semaphore.Release(); // 释放信号量
                    }
                }, token.Cancellation);
                offset += chunkSize;
            }
            // 合并文件
            return Task.CompletedTask;
        }

        public void Stop()
        {
        }

        public void Dispose()
        {
        }
    }
}
