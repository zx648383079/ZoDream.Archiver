using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ZoDream.Shared.Net
{
    public class LimitedScheduler : TaskScheduler, IRequestScheduler
    {

        public LimitedScheduler(INetService service, int maxDegreeOfParallelism)
        {
            _service = service;
            _maxDegreeOfParallelism = maxDegreeOfParallelism;
            _factory = new(this);
        }

        [ThreadStatic]
        private static bool _currentThreadIsProcessingItems;
        private readonly int _maxDegreeOfParallelism;
        private int _delegatesQueuedOrRunning = 0;
        private readonly ConcurrentQueue<Task> _tasks = new();
        private readonly TaskFactory _factory;
        private readonly INetService _service;
        private readonly CancellationTokenSource _tokenSource = new();
        protected override IEnumerable<Task> GetScheduledTasks() => [.. _tasks];
        public override int MaximumConcurrencyLevel => _maxDegreeOfParallelism;

        public void Execute(RequestContext request)
        {
            Execute(async () => {
                request.Token.Emit(new RequestChangedEventArgs()
                {
                    Status = RequestStatus.Sending
                });
                var res = await _service.SendAsync(request);
                if (res.StatusCode != HttpStatusCode.OK)
                {
                    request.Token.Emit(new RequestChangedEventArgs()
                    {
                        Status = RequestStatus.Occurred
                    });
                    return;
                }
                var fileName = _service.GetFileName(res);
                var length = _service.GetContentLength(res);
                var output = Path.Combine(request.OutputFolder, fileName);
                request.Token.Emit(new RequestChangedEventArgs()
                {
                    Length = length,
                    FileName = fileName,
                    OutputPath = output,
                    Status = RequestStatus.Receiving
                });
                using var fs = new ConcurrentStream(File.Create(output));
                using INetReceiver receiver = length > 100000 && _service.GetAcceptRange(res) ?
                new ChunkReceiver(_service, res, request) : new NetReceiver(res, request);
                await receiver.StartAsync(_tokenSource.Token);
            });
        }

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
