
namespace UnityEngine
{
    public struct StreamedCurveKey
    {
        public int Index;
        public float[] Coeff;

        public float Value;
        public float OutSlope;
        public float InSlope;
    }

    public struct StreamedFrame
    {
        public float Time;
        public StreamedCurveKey[] KeyList;
    }
    public struct StreamedClip
    {
        public uint[] Data;
        public uint CurveCount;

    }

}
