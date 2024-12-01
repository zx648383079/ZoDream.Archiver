using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.BundleFiles
{
    public interface IBundleHeader
    {
        public void Read(IBundleBinaryReader reader);
    }
}
