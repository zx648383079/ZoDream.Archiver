using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal abstract class UIBehavior(UIReader reader) : UIComponent(reader)
    {
        public byte m_Enabled;

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            m_Enabled = reader.ReadByte();
            reader.AlignStream();
        }
    }
}
