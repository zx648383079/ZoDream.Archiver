using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Handle
    {
        public XForm<Vector3> m_X;
        public uint m_ParentHumanIndex;
        public uint m_ID;

        public Handle(UIReader reader)
        {
            m_X = reader.ReadXForm();
            m_ParentHumanIndex = reader.ReadUInt32();
            m_ID = reader.ReadUInt32();
        }
    }

}
