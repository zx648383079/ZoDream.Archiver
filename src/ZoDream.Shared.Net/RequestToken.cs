using System.Threading;
using System.Threading.Tasks;

namespace ZoDream.Shared.Net
{
    public struct RequestToken(RequestTokenSource source)
    {
        public readonly bool IsPaused => source.IsPaused;

        public readonly CancellationToken Cancellation => source.CancellationToken;

        public readonly bool IsCancellationRequested => source.IsCancellationRequested;

        public readonly Task WaitWhilePausedAsync()
        {
            return source.WaitWhilePausedAsync();
        }
    }
}
