using System.Threading;
using System.Threading.Tasks;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleToken
    {
        public bool IsPaused { get; }

        public CancellationToken Cancellation { get; }

        public bool IsCancellationRequested { get; }

        public Task WaitWhilePausedAsync();

        public void Emit(BundleChangedEventArgs args);

        public void Emit(long received);
    }
}
