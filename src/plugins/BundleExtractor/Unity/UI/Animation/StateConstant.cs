using System.Collections.Generic;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class StateConstant : IElementLoader
    {
        public List<TransitionConstant> m_TransitionConstantArray;
        public int[] m_BlendTreeConstantIndexArray;
        public List<LeafInfoConstant> m_LeafInfoArray;
        public List<BlendTreeConstant> m_BlendTreeConstantArray;
        public uint m_NameID;
        public uint m_PathID;
        public uint m_FullPathID;
        public uint m_TagID;
        public uint m_SpeedParamID;
        public uint m_MirrorParamID;
        public uint m_CycleOffsetParamID;
        public float m_Speed;
        public float m_CycleOffset;
        public bool m_IKOnFeet;
        public bool m_WriteDefaultValues;
        public bool m_Loop;
        public bool m_Mirror;

        public void Read(IBundleBinaryReader reader)
        {
            ReadBase(reader);

            reader.AlignStream();
        }

        public void ReadBase(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();

            int numTransistions = reader.ReadInt32();
            m_TransitionConstantArray = [];
            for (int i = 0; i < numTransistions; i++)
            {
                m_TransitionConstantArray.Add(new TransitionConstant(reader));
            }

            m_BlendTreeConstantIndexArray = reader.ReadArray(r => r.ReadInt32());

            if (version.LessThan(5, 2)) //5.2 down
            {
                int numInfos = reader.ReadInt32();
                m_LeafInfoArray = [];
                for (int i = 0; i < numInfos; i++)
                {
                    m_LeafInfoArray.Add(new LeafInfoConstant(reader));
                }
            }

            int numBlends = reader.ReadInt32();
            m_BlendTreeConstantArray = new List<BlendTreeConstant>();
            for (int i = 0; i < numBlends; i++)
            {
                m_BlendTreeConstantArray.Add(new BlendTreeConstant(reader));
            }

            m_NameID = reader.ReadUInt32();
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                m_PathID = reader.ReadUInt32();
            }
            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                m_FullPathID = reader.ReadUInt32();
            }

            m_TagID = reader.ReadUInt32();
            if (version.GreaterThanOrEquals(5, 1)) //5.1 and up
            {
                m_SpeedParamID = reader.ReadUInt32();
                m_MirrorParamID = reader.ReadUInt32();
                m_CycleOffsetParamID = reader.ReadUInt32();
            }

            if (version.GreaterThanOrEquals(2017, 2)) //2017.2 and up
            {
                var m_TimeParamID = reader.ReadUInt32();
            }

            m_Speed = reader.ReadSingle();
            if (version.GreaterThanOrEquals(4, 1)) //4.1 and up
            {
                m_CycleOffset = reader.ReadSingle();
            }
            m_IKOnFeet = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(5)) //5.0 and up
            {
                m_WriteDefaultValues = reader.ReadBoolean();
            }

            m_Loop = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(4, 1)) //4.1 and up
            {
                m_Mirror = reader.ReadBoolean();
            }

            
        }
    }
}
