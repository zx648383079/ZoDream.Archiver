namespace ZoDream.BundleExtractor.Unity.UI
{
    public abstract class EditorExtension : UIObject
    {
        protected EditorExtension(UIReader reader) : base(reader)
        {
            if (reader.Platform == BuildTarget.NoTarget)
            {
                var m_PrefabParentObject = new PPtr<EditorExtension>(reader);
                var m_PrefabInternal = new PPtr<UIObject>(reader); //PPtr<Prefab>
            }
        }
    }
}
