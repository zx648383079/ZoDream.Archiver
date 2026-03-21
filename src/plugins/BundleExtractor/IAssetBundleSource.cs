using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor
{
    public interface IAssetBundleSource: IBundleSource
    {
        public string AliasName { get; }
        public bool IsMatch();
    }
}
