using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SamplerParameter
    {
        public uint sampler;
        public int bindPoint;

        public SamplerParameter(IBundleBinaryReader reader)
        {
            sampler = reader.ReadUInt32();
            bindPoint = reader.ReadInt32();
        }
    }
}
