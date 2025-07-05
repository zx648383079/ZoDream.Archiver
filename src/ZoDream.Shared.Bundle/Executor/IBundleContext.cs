using System.Diagnostics.CodeAnalysis;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleContext
    {
        public IEntryService Service { get; }

        public void Enqueue(IBundleRequest request);

        public bool TryDequeue([NotNullWhen(true)] out IBundleRequest? request);
    }
}
