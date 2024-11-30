using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class ValueArrayConstant
    {
        public List<ValueConstant> m_ValueArray;

        public ValueArrayConstant(UIReader reader)
        {
            int numVals = reader.ReadInt32();
            m_ValueArray = new List<ValueConstant>();
            for (int i = 0; i < numVals; i++)
            {
                m_ValueArray.Add(new ValueConstant(reader));
            }
        }
    }
}
