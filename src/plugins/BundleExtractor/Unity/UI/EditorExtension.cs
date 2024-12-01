using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal abstract class EditorExtension(UIReader reader) : UIObject(reader)
    {
        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            if (_reader.Platform == BuildTarget.NoTarget)
            {
                var m_PrefabParentObject = new PPtr<EditorExtension>(_reader);
                
                var m_PrefabInternal = new PPtr<UIObject>(_reader); //PPtr<Prefab>
            }
        }
    }
}
