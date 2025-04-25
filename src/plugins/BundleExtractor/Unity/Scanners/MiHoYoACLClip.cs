using UnityEngine;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal class MiHoYoACLClip : ACLClip
    {
        public uint m_CurveCount;
        public uint m_ConstCurveCount;

        public byte[] m_ClipData;

        public override bool IsSet => m_ClipData is not null && m_ClipData.Length > 0;
        public override uint CurveCount => m_CurveCount;

        public MiHoYoACLClip()
        {
            m_CurveCount = 0;
            m_ConstCurveCount = 0;
            m_ClipData = [];
        }

    }

}
