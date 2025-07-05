using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Net
{
    public class ChunkReceiver(
        ITemporaryStorage storage, 
        INetService service,
        HttpResponseMessage response,
        IRequestContext request, 
        Stream output) : INetReceiver
    {
        const long ChunkSize = 1024 * 1024;
        private readonly SemaphoreSlim _semaphore = new(4);
        private readonly Mutex _outputLock = new();

        public void Pause()
        {
        }

        public void Resume()
        {
        }

        public Task StartAsync(IBundleToken token)
        {
            var length = service.GetContentLength(response);
            var offset = 0L;
            var taskItems = new List<Task>();
            while (offset < length && !token.IsCancellationRequested)
            {
                var currentOffset = offset;
                var currentLength = Math.Min(ChunkSize, length - offset);
                async Task cb()
                {
                    await _semaphore.WaitAsync(token.Cancellation);
                    try
                    {
                        using var fs = await storage.CreateAsync($"_temp{offset}");
                        if (currentOffset == 0)
                        {
                            using var input = await service.ReadAsStreamAsync(response);
                            await NetService.CopyToAsync(input, currentLength, fs, token);
                        }
                        else
                        {
                            using var chunk = await service.SendAsync(request,
                                new RangeHeaderValue(currentOffset, currentOffset + currentLength));
                            await service.SaveAsAsync(chunk, fs, token);
                        }
                        _outputLock.WaitOne();
                        fs.Seek(0, SeekOrigin.Begin);
                        output.Seek(offset, SeekOrigin.Begin);
                        fs.CopyTo(output);
                        _outputLock.ReleaseMutex();
                    }
                    finally
                    {
                        _semaphore.Release(); // 释放信号量
                    }
                }
                taskItems.Add(cb());
                offset += ChunkSize;
            }
            Task.WaitAll(taskItems, token.Cancellation);
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
