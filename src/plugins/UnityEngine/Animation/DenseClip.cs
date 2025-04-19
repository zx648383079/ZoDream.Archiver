namespace UnityEngine
{
    public class DenseClip
    {
        public int FrameCount { get; set; }
        public uint CurveCount { get; set; }
        public float SampleRate { get; set; }
        public float BeginTime { get; set; }
        public float[] SampleArray { get; set; }
    }
}
