namespace ZoDream.BundleExtractor.Unity.UI
{
    public abstract class UIComponent : EditorExtension
    {
        public PPtr<GameObject> m_GameObject;

        protected UIComponent(UIReader reader)
            : base(reader)
        {
            m_GameObject = new PPtr<GameObject>(reader);
        }
    }
}
