namespace ZoDream.Live2dExporter.Models
{
    internal class JsonPhysicsOutput
    {
        public JsonPhysicsParameter Destination { get; set; }
        public int VertexIndex { get; set; }
        public float Scale { get; set; }
        public float Weight { get; set; }
        public string Type { get; set; }
        public bool Reflect { get; set; }
    }
}
