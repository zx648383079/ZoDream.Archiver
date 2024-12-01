using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class BlendTreeNodeConstant : IElementLoader
    {
        public uint m_BlendType;
        public uint m_BlendEventID;
        public uint m_BlendEventYID;
        public uint[] m_ChildIndices;
        public float[] m_ChildThresholdArray;
        public Blend1dDataConstant m_Blend1dData;
        public Blend2dDataConstant m_Blend2dData;
        public BlendDirectDataConstant m_BlendDirectData;
        public uint m_ClipID;
        public uint m_ClipIndex;
        public float m_Duration;
        public float m_CycleOffset;
        public bool m_Mirror;

        public void Read(IBundleBinaryReader reader)
        {
            ReadBase(reader);
            var version = reader.Get<UnityVersion>();
            if (version.GreaterThanOrEquals(4, 1, 3)) //4.1.3 and up
            {
                m_CycleOffset = reader.ReadSingle();
                m_Mirror = reader.ReadBoolean();
                reader.AlignStream();
            }
        }

        public void ReadBase(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();

            if (version.GreaterThanOrEquals(4, 1)) //4.1 and up
            {
                m_BlendType = reader.ReadUInt32();
            }
            m_BlendEventID = reader.ReadUInt32();
            if (version.GreaterThanOrEquals(4, 1)) //4.1 and up
            {
                m_BlendEventYID = reader.ReadUInt32();
            }
            m_ChildIndices = reader.ReadArray(r => r.ReadUInt32());
            if (version.LessThan(4, 1)) //4.1 down
            {
                m_ChildThresholdArray = reader.ReadArray(r => r.ReadSingle());
            }

            if (version.GreaterThanOrEquals(4, 1)) //4.1 and up
            {
                m_Blend1dData = new Blend1dDataConstant(reader);
                m_Blend2dData = new Blend2dDataConstant(reader);
            }

            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                m_BlendDirectData = new BlendDirectDataConstant(reader);
            }

            m_ClipID = reader.ReadUInt32();
            if (version.Major == 4 && version.Minor >= 5) //4.5 - 5.0
            {
                m_ClipIndex = reader.ReadUInt32();
            }

            m_Duration = reader.ReadSingle();

            
        }
    }
}
