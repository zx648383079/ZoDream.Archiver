using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class ValueDelta
    {
        public float m_Start;
        public float m_Stop;

        public ValueDelta(UIReader reader)
        {
            m_Start = reader.ReadSingle();
            m_Stop = reader.ReadSingle();
        }
    }
}
