namespace ZoDream.Shared.Bundle
{
    public interface IBundleLoader
    {
        public string AliasName { get; }
        public bool TryLoad(IBundleSource fileItems, IBundleOptions options);
    }
}
