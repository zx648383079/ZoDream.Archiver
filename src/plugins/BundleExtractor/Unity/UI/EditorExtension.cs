namespace ZoDream.BundleExtractor.Unity.UI
{
    public abstract class EditorExtension : UIObject
    {
        protected EditorExtension(UIReader reader) 
            : this(reader, true)
        {
            
        }

        protected EditorExtension(UIReader reader, bool isReadable)
            : base(reader, isReadable)
        {
            if (isReadable && reader.Platform == BuildTarget.NoTarget)
            {
                var m_PrefabParentObject = new PPtr<EditorExtension>(reader);
                var m_PrefabInternal = new PPtr<UIObject>(reader); //PPtr<Prefab>
            }
        }
    }
}
