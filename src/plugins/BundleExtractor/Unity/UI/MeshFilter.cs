using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class MeshFilter : UIComponent
    {
        public PPtr<Mesh> m_Mesh;

        public MeshFilter(UIReader reader) : base(reader)
        {
            m_Mesh = new PPtr<Mesh>(reader);
        }
    }
}
