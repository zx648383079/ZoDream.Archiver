using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Hash128
    {
        public byte[] bytes;

        public Hash128(IBundleBinaryReader reader)
        {
            bytes = reader.ReadBytes(16);
        }
    }

}
