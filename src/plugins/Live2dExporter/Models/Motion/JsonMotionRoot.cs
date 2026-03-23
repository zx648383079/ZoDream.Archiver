namespace ZoDream.Live2dExporter.Models
{
    internal class JsonMotionRoot
    {
        public int Version { get; set; }

        public JsonMotionMeta Meta { get; set; }

        public JsonMotionCurve[] Curves { get; set; }
        public JsonMotionUserData[] UserData { get; set; }
    }
}
