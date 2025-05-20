using System;
using System.Threading;

namespace ZoDream.Shared.Net
{
    public class SemaphoreScheduler : IRequestScheduler
    {
        public SemaphoreScheduler(INetService service, int maxDegreeOfParallelism)
        {
            _service = service;
            _semaphore = new(maxDegreeOfParallelism);
        }

        private readonly SemaphoreSlim _semaphore;
        private readonly INetService _service;
        private readonly CancellationTokenSource _tokenSource = new();

        public void Execute(RequestContext request)
        {
            Execute(() => {
                _service.SendAsync(request);
            });
        }

        public void Execute(Action action)
        {
            var token = _tokenSource.Token;
            ThreadPool.QueueUserWorkItem(async _ => {
                await _semaphore.WaitAsync(token);
                try
                {
                    action.Invoke();
                }
                finally
                {
                    _semaphore.Release(); // 释放信号量
                }
            });
        }

        public void Cancel()
        {
            _tokenSource.Cancel();
        }


        public void Dispose()
        {
            _semaphore.Dispose();
        }
    }
}
