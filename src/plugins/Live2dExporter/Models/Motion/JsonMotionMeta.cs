namespace ZoDream.Live2dExporter.Models
{
    internal class JsonMotionMeta
    {
        public float Duration { get; set; }
        public float Fps { get; set; }
        public bool Loop { get; set; }
        public bool AreBeziersRestricted { get; set; }
        public float FadeInTime { get; set; }
        public float FadeOutTime { get; set; }
        public int CurveCount { get; set; }
        public int TotalSegmentCount { get; set; }
        public int TotalPointCount { get; set; }
        public int UserDataCount { get; set; }
        public int TotalUserDataSize { get; set; }
    }
}
