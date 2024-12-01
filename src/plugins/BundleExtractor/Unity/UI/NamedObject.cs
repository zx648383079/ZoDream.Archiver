using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class NamedObject(UIReader reader) : EditorExtension(reader)
    {
        public string m_Name;

        public override string Name => m_Name;

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            m_Name = reader.ReadAlignedString();
        }
    }
}
