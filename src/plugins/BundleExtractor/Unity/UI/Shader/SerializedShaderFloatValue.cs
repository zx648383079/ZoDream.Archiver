using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SerializedShaderFloatValue
    {
        public float val;
        public string name;

        public SerializedShaderFloatValue(IBundleBinaryReader reader)
        {
            val = reader.ReadSingle();
            name = reader.ReadAlignedString();
        }
    }

}
