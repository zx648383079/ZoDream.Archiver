namespace ZoDream.Live2dExporter.Models
{
    public class JsonMotionMappingParameter
    {
        public string Type { get; set; }

        public string Id { get; set; }

        public JsonMotionMappingTarget[] Targets { get; set; }
    }
}