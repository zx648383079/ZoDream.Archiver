using System.Numerics;

namespace ZoDream.Live2dExporter.Models
{
    internal class JsonPhysicsVertex
    {
        public Vector2 Position { get; set; }
        public float Mobility { get; set; }
        public float Delay { get; set; }
        public float Acceleration { get; set; }
        public float Radius { get; set; }
    }
}
