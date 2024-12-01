using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class MeshFilter(UIReader reader) : UIComponent(reader)
    {
        public PPtr<Mesh> m_Mesh;

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            m_Mesh = new PPtr<Mesh>(reader);
        }
    }
}
