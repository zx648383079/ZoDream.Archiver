using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class LayerConstant
    {
        public uint m_StateMachineIndex;
        public uint m_StateMachineMotionSetIndex;
        public HumanPoseMask m_BodyMask;
        public SkeletonMask m_SkeletonMask;
        public uint m_Binding;
        public int m_LayerBlendingMode;
        public float m_DefaultWeight;
        public bool m_IKPass;
        public bool m_SyncedLayerAffectsTiming;

        public LayerConstant(UIReader reader)
        {
            var version = reader.Version;

            m_StateMachineIndex = reader.ReadUInt32();
            m_StateMachineMotionSetIndex = reader.ReadUInt32();
            m_BodyMask = new HumanPoseMask(reader);
            m_SkeletonMask = new SkeletonMask(reader);
            if (reader.IsLoveAndDeepSpace())
            {
                var m_GenericMask = new SkeletonMask(reader);
            }
            m_Binding = reader.ReadUInt32();
            m_LayerBlendingMode = reader.ReadInt32();
            if (version.GreaterThanOrEquals(4, 2)) //4.2 and up
            {
                m_DefaultWeight = reader.ReadSingle();
            }
            m_IKPass = reader.ReadBoolean();
            if (version.GreaterThanOrEquals(4, 2)) //4.2 and up
            {
                m_SyncedLayerAffectsTiming = reader.ReadBoolean();
            }
            reader.AlignStream();
        }

    }
}
