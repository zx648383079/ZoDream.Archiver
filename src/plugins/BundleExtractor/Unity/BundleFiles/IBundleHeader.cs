using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.BundleFiles
{
    public interface IBundleHeader
    {
        public void Read(EndianReader reader);
    }
}
