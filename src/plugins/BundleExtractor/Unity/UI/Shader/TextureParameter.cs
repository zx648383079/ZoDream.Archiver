using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class TextureParameter
    {
        public int m_NameIndex;
        public int m_Index;
        public int m_SamplerIndex;
        public sbyte m_Dim;

        public TextureParameter(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();

            m_NameIndex = reader.ReadInt32();
            m_Index = reader.ReadInt32();
            m_SamplerIndex = reader.ReadInt32();
            if (version.GreaterThanOrEquals(2017, 3)) //2017.3 and up
            {
                var m_MultiSampled = reader.ReadBoolean();
            }
            m_Dim = reader.ReadSByte();
            reader.AlignStream();
        }
    }

}
