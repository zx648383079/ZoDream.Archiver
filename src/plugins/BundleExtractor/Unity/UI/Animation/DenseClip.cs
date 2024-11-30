using System.IO;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class DenseClip
    {
        public int m_FrameCount;
        public uint m_CurveCount;
        public float m_SampleRate;
        public float m_BeginTime;
        public float[] m_SampleArray;
        public DenseClip() { }

        public DenseClip(UIReader reader)
        {
            m_FrameCount = reader.ReadInt32();
            m_CurveCount = reader.ReadUInt32();
            m_SampleRate = reader.ReadSingle();
            m_BeginTime = reader.ReadSingle();
            m_SampleArray = reader.ReadArray(r => r.ReadSingle());
        }
        public static DenseClip ParseGI(UIReader reader)
        {
            var denseClip = new DenseClip();

            denseClip.m_FrameCount = reader.ReadInt32();
            denseClip.m_CurveCount = reader.ReadUInt32();
            denseClip.m_SampleRate = reader.ReadSingle();
            denseClip.m_BeginTime = reader.ReadSingle();

            var denseClipCount = (int)reader.ReadUInt64();
            var denseClipOffset = reader.Position + reader.ReadInt64();
            if (denseClipOffset > reader.Length)
            {
                throw new IOException("Offset outside of range");
            }

            var pos = reader.Position;
            reader.Position = denseClipOffset;

            denseClip.m_SampleArray = reader.ReadArray(denseClipCount, r => r.ReadSingle());

            reader.Position = pos;

            return denseClip;
        }
    }
}
