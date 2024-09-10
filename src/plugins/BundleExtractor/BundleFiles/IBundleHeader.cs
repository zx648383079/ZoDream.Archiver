using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.BundleFiles
{
    public interface IBundleHeader
    {
        public void Read(EndianReader reader);
    }
}
