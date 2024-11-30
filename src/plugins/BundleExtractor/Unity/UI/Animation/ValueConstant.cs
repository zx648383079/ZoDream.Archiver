using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class ValueConstant
    {
        public uint m_ID;
        public uint m_TypeID;
        public uint m_Type;
        public uint m_Index;

        public ValueConstant(UIReader reader)
        {
            var version = reader.Version;
            m_ID = reader.ReadUInt32();
            if (version.LessThan(5, 5))//5.5 down
            {
                m_TypeID = reader.ReadUInt32();
            }
            m_Type = reader.ReadUInt32();
            m_Index = reader.ReadUInt32();
        }
    }
}
