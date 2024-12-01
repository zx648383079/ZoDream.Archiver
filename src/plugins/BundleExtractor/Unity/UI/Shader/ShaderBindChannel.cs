using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class ShaderBindChannel
    {
        public sbyte source;
        public sbyte target;

        public ShaderBindChannel(IBundleBinaryReader reader)
        {
            source = reader.ReadSByte();
            target = reader.ReadSByte();
        }
    }

}
