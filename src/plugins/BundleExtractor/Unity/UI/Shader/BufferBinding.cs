using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class BufferBinding
    {
        public int m_NameIndex;
        public int m_Index;
        public int m_ArraySize;

        public BufferBinding(UIReader reader)
        {
            var version = reader.Version;

            m_NameIndex = reader.ReadInt32();
            m_Index = reader.ReadInt32();
            if (version.GreaterThanOrEquals(2020, 1)) //2020.1 and up
            {
                m_ArraySize = reader.ReadInt32();
            }
        }
    }

}
