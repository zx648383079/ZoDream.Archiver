using System;

namespace ZoDream.Shared.Net
{
    public interface IRequestScheduler : IDisposable
    {
        public void Execute(RequestContext request);
        public void Execute(Action action);
        public void Cancel();

    }
}
