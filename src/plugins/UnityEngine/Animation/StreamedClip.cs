
namespace UnityEngine
{
    public class StreamedCurveKey
    {
        public int Index;
        public float[] Coeff;

        public float Value;
        public float OutSlope;
        public float InSlope;
    }

    public class StreamedFrame
    {
        public float Time;
        public StreamedCurveKey[] KeyList;
    }
    public class StreamedClip
    {
        public StreamedFrame[] Data;
        public uint CurveCount;

    }

}
