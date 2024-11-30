namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class MHYACLClip : ACLClip
    {
        public uint m_CurveCount;
        public uint m_ConstCurveCount;

        public byte[] m_ClipData;

        public override bool IsSet => m_ClipData is not null && m_ClipData.Length > 0;
        public override uint CurveCount => m_CurveCount;

        public MHYACLClip()
        {
            m_CurveCount = 0;
            m_ConstCurveCount = 0;
            m_ClipData = [];
        }
        public override void Read(UIReader reader)
        {
            var byteCount = reader.ReadInt32();

            if (reader.IsSRGroup())
            {
                byteCount *= 4;
            }

            m_ClipData = reader.ReadBytes(byteCount);
            reader.AlignStream();

            m_CurveCount = reader.ReadUInt32();

            if (reader.IsSRGroup())
            {
                m_ConstCurveCount = reader.ReadUInt32();
            }
        }
    }

}
