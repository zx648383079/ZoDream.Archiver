using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class BlendDirectDataConstant
    {
        public uint[] m_ChildBlendEventIDArray;
        public bool m_NormalizedBlendValues;

        public BlendDirectDataConstant(UIReader reader)
        {
            m_ChildBlendEventIDArray = reader.ReadArray(r => r.ReadUInt32());
            m_NormalizedBlendValues = reader.ReadBoolean();
            reader.AlignStream();
        }
    }
}
