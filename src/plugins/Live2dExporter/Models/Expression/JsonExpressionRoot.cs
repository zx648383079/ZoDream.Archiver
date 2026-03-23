namespace ZoDream.Live2dExporter.Models
{
    internal class JsonExpressionRoot
    {
        public string Type { get; set; }
        public float FadeInTime { get; set; }
        public float FadeOutTime { get; set; }
        public JsonExpressionParameter[] Parameters { get; set; }
    }
}
