using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal abstract class UIComponent(UIReader reader) : EditorExtension(reader)
    {
        public PPtr<GameObject> m_GameObject;

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            m_GameObject = new PPtr<GameObject>(_reader);
        }
    }
}
