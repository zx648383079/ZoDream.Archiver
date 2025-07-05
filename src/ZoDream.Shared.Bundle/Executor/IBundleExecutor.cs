using System.Threading.Tasks;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleExecutor
    {
        public bool CanExecute(IBundleRequest request);
        public Task ExecuteAsync(IBundleRequest request, IBundleContext context);
    }
}