using System.Threading;
using System.Threading.Tasks;

namespace ZoDream.Shared.Bundle
{
    public struct BundleToken(BundleTokenSource source) : IBundleToken
    {
        public readonly bool IsPaused => source.IsPaused;

        public readonly CancellationToken Cancellation => source.CancellationToken;

        public readonly bool IsCancellationRequested => source.IsCancellationRequested;

        public readonly Task WaitWhilePausedAsync()
        {
            return source.WaitWhilePausedAsync();
        }

        public readonly void Emit(BundleChangedEventArgs args)
        {
            source.Emit(args);
        }

        public readonly void Emit(long received)
        {
            source.Emit(received);
        }
    }
}
