using System.Threading;
using System.Threading.Tasks;

namespace ZoDream.Shared.Bundle
{
    public sealed class BundleTokenSource 
    {
        private readonly CancellationTokenSource _cancellationSource = new();
        private volatile TaskCompletionSource<bool>? _paused;
        public bool IsPaused { get; private set; }
        public bool IsCancellationRequested => _cancellationSource.IsCancellationRequested;

        public BundleToken Token => new(this);
        public CancellationToken CancellationToken => _cancellationSource.Token;

        public event BundleChangedEventHandler? RequestChanged;
        public event BundleProgressEventHandler? ProgressChanged;

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

        internal void Emit(BundleChangedEventArgs args)
        {
            RequestChanged?.Invoke(args);
        }

        internal void Emit(long received)
        {
            ProgressChanged?.Invoke(received);
        }
    }
}
