using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class ConstantClip
    {
        public float[] data;
        public ConstantClip() { }

        public ConstantClip(IBundleBinaryReader reader)
        {
            data = reader.ReadArray(r => r.ReadSingle());
        }
    }
}
