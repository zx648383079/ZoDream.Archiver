namespace ZoDream.BundleExtractor.Unity.UI
{
    internal abstract class UIBehavior : UIComponent
    {
        public byte m_Enabled;

        protected UIBehavior(UIReader reader)
            : base(reader)
        {
            m_Enabled = reader.ReadByte();
            reader.AlignStream();
        }
    }
}
