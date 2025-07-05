using System;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleScheduler : IDisposable
    {
        public void Execute(Action action);
        public void Cancel();

    }
}
