using System.Threading;
using System.Threading.Tasks;

namespace ZoDream.Shared.Net
{
    public sealed class RequestTokenSource 
    {
        private readonly CancellationTokenSource _cancellationSource = new();
        private volatile TaskCompletionSource<bool>? _paused;
        public bool IsPaused { get; private set; }
        public bool IsCancellationRequested => _cancellationSource.IsCancellationRequested;

        public RequestToken Token => new(this);


        public CancellationToken CancellationToken => _cancellationSource.Token;

        public void Cancel()
        {
            if (!IsCancellationRequested)
            {
                Resume();
                _cancellationSource.Cancel();
            }
        }

        public void Pause()
        {
            if (!IsPaused)
            {
                IsPaused = true;
                _paused = new TaskCompletionSource<bool>();
            }
        }

        public void Resume()
        {
            if (IsPaused)
            {
                IsPaused = false;
                _paused?.TrySetResult(true);
            }
        }

        public Task WaitWhilePausedAsync()
        {
            return IsPaused && !IsCancellationRequested ? _paused!.Task : Task.CompletedTask;
        }
    }
}
