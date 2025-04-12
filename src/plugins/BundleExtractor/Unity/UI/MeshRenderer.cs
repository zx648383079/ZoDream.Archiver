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
        public override void Associated(IDependencyBuilder? builder)
        {
            base.Associated(builder);
            builder?.AddDependencyEntry(_reader.FullPath, FileID, m_AdditionalVertexStreams.m_PathID);
        }
    }
}
