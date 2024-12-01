using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class BufferBinding
    {
        public int m_NameIndex;
        public int m_Index;
        public int m_ArraySize;

        public BufferBinding(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();

            m_NameIndex = reader.ReadInt32();
            m_Index = reader.ReadInt32();
            if (version.GreaterThanOrEquals(2020, 1)) //2020.1 and up
            {
                m_ArraySize = reader.ReadInt32();
            }
        }
    }

}
