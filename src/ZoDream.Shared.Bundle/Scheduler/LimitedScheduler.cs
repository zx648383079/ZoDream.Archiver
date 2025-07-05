using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle
{
    public class LimitedScheduler : TaskScheduler, IBundleScheduler
    {

        public LimitedScheduler(IEntryService service, int maxDegreeOfParallelism)
        {
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
            _factory = new(this);
        }

        [ThreadStatic]
        private static bool _currentThreadIsProcessingItems;
        private readonly int _maxDegreeOfParallelism;
        private int _delegatesQueuedOrRunning = 0;
        private readonly ConcurrentQueue<Task> _tasks = new();
        private readonly TaskFactory _factory;
        private readonly CancellationTokenSource _tokenSource = new();
        protected override IEnumerable<Task> GetScheduledTasks() => [.. _tasks];
        public override int MaximumConcurrencyLevel => _maxDegreeOfParallelism;

        public void Execute(Action action)
        {
            _factory.StartNew(action, _tokenSource.Token);
        }

        public void Cancel()
        {
            _tokenSource.Cancel();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            if (!_currentThreadIsProcessingItems)
            {
                return false;
            }
            return taskWasPreviouslyQueued ? TryDequeue(task) && TryExecuteTask(task)
                                          : TryExecuteTask(task);
        }

        protected override void QueueTask(Task task)
        {
            _tasks.Enqueue(task);
            if (Interlocked.Increment(ref _delegatesQueuedOrRunning) <= _maxDegreeOfParallelism)
            {
                ThreadPool.UnsafeQueueUserWorkItem(_ => ProcessTasks(), null);
            }
        }

        private void ProcessTasks()
        {
            _currentThreadIsProcessingItems = true;
            try
            {
                while (true)
                {
                    if (_tasks.TryDequeue(out var task))
                    {
                        TryExecuteTask(task);
                    }
                    else
                    {
                        if (Interlocked.Decrement(ref _delegatesQueuedOrRunning) == 0)
                        {
                            break;
                        }
                        _currentThreadIsProcessingItems = false;
                        Thread.Yield();
                        _currentThreadIsProcessingItems = true;
                    }
                }
            }
            finally
            {
                _currentThreadIsProcessingItems = false;
            }
        }

        public void Dispose()
        {
            _tokenSource.Cancel();
        }
    }
}
