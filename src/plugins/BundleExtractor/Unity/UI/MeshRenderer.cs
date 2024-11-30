using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class MeshRenderer : UIRenderer
    {
        public PPtr<Mesh> m_AdditionalVertexStreams;
        public MeshRenderer(UIReader reader) : base(reader)
        {
            m_AdditionalVertexStreams = new PPtr<Mesh>(reader);
        }
    }
}
