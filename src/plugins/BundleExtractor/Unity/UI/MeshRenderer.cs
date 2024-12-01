using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class MeshRenderer(UIReader reader) : UIRenderer(reader)
    {
        public PPtr<Mesh> m_AdditionalVertexStreams;

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            m_AdditionalVertexStreams = new PPtr<Mesh>(reader);
        }
    }
}
