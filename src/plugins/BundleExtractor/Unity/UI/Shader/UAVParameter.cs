using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class UAVParameter
    {
        public int m_NameIndex;
        public int m_Index;
        public int m_OriginalIndex;

        public UAVParameter(EndianReader reader)
        {
            m_NameIndex = reader.ReadInt32();
            m_Index = reader.ReadInt32();
            m_OriginalIndex = reader.ReadInt32();
        }
    }

}
