namespace ZoDream.Live2dExporter.Models
{
    internal class JsonPhysicsRoot
    {
        public int Version { get; set; }
        public JsonPhysicsMeta Meta { get; set; }
        public JsonPhysicsSettings[] PhysicsSettings { get; set; }

    }
}
