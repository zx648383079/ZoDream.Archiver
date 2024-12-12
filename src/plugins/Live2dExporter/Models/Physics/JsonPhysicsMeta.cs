namespace ZoDream.Live2dExporter.Models
{
    internal class JsonPhysicsMeta
    {
        public int PhysicsSettingCount { get; set; }
        public int TotalInputCount { get; set; }
        public int TotalOutputCount { get; set; }
        public int VertexCount { get; set; }
        public JsonPhysicsEffectiveForces EffectiveForces { get; set; }
        public JsonPhysicsDictionary[] PhysicsDictionary { get; set; }
    }
}
