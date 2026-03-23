namespace ZoDream.Live2dExporter.Models
{
    internal class JsonMotionCurve
    {
        public string Target { get; set; }
        public string Id { get; set; }
        public float FadeInTime { get; set; }
        public float FadeOutTime { get; set; }
        public float[] Segments { get; set; }
    }
}