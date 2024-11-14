namespace ZoDream.Shared.Bundle
{
    public interface IBundlePlatform
    {
        public bool TryLoad(IBundleSource fileItems, IBundleOptions options);
    }
}
