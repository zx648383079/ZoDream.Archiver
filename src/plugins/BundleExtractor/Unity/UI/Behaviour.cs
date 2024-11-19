namespace ZoDream.BundleExtractor.Unity.UI
{
    public abstract class UIBehaviour : UIComponent
    {
        public byte m_Enabled;

        protected UIBehaviour(UIReader reader)
            : base(reader)
        {
            m_Enabled = reader.ReadByte();
            reader.AlignStream();
        }
    }
}
