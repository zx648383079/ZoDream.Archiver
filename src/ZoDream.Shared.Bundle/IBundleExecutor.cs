namespace ZoDream.Shared.Bundle
{
    public interface IBundleExecutor
    {
        public bool CanExecute(IBundleRequest request);
        public void Execute(IBundleRequest request, IBundleContext context);
    }
}