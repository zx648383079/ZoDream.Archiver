namespace ZoDream.BundleExtractor.Unity.UI
{
    internal abstract class UIComponent : EditorExtension
    {
        public PPtr<GameObject> m_GameObject;

        protected UIComponent(UIReader reader)
            : base(reader)
        {
            m_GameObject = new PPtr<GameObject>(reader);
        }
    }
}
