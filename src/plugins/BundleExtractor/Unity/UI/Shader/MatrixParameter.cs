using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class MatrixParameter
    {
        public int m_NameIndex;
        public int m_Index;
        public int m_ArraySize;
        public sbyte m_Type;
        public sbyte m_RowCount;

        public MatrixParameter(UIReader reader)
        {
            m_NameIndex = reader.ReadInt32();
            m_Index = reader.ReadInt32();
            m_ArraySize = reader.ReadInt32();
            m_Type = reader.ReadSByte();
            m_RowCount = reader.ReadSByte();
            reader.AlignStream();
        }
    }

}
