using System;
using System.Threading;

namespace ZoDream.Shared.Bundle
{
    public class SemaphoreScheduler : IBundleScheduler
    {
        public SemaphoreScheduler(int maxDegreeOfParallelism)
        {
            _semaphore = new(maxDegreeOfParallelism);
        }

        private readonly SemaphoreSlim _semaphore;
        private readonly CancellationTokenSource _tokenSource = new();


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
