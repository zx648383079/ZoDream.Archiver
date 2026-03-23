namespace ZoDream.Live2dExporter.Models
{
    internal class JsonExpressionParameter
    {
        public string Id { get; set; }
        public float Value { get; set; }
        public JsonBlendType Blend { get; set; }
    }

    internal enum JsonBlendType
    {
        Add,
        Multiply,
        Overwrite,
    }
}