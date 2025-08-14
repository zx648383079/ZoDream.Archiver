using System.IO;
using UnityEngine;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal class TuanJieQuantizedClip: ACLClip
    {
        public int m_FrameCount;
        public uint m_CurveCount;
        public float m_SampleRate;
        public float m_BeginTime;
        public uint m_NumStatic;
        public uint m_NumDynamic;
        public uint m_TypeOffset;
        public uint m_IndicesOffset;
        public uint m_StaticOffset;
        public uint m_FrameSize;
        public uint m_DynamicOffset;
        public Stream m_Data;
    }

    internal class TuanJiePredictClip : ACLClip
    {
        public int m_FrameCount;
        public uint m_CurveCount;
        public float m_SampleRate;
        public float m_BeginTime;
        public uint m_NumStatic;
        public uint m_NumDynamic;
        public uint m_TypeOffset;
        public uint m_IndicesOffset;
        public uint m_StaticOffset;
        public uint m_RangeOffset;
        public uint m_BitCntOffset;
        public uint m_PredictBlockOffset;
        public uint m_ValueOffsetPerCurveOffset;
        public uint m_ValueOffset;
        public Stream m_Data;
    }
}
