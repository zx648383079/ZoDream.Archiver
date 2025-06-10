using System.Diagnostics.CodeAnalysis;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleContext
    {

        public void Enqueue(IBundleRequest request);

        public bool TryDequeue([NotNullWhen(true)] out IBundleRequest? request);
    }
}
