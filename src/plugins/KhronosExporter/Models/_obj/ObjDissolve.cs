namespace ZoDream.KhronosExporter.Models
{
    internal class ObjDissolve
    {
        /// <summary>
        /// A factor of 1.0 is fully opaque.
        /// </summary>
        public double Factor { get; set; }
        /// <summary>
        /// d -halo 0.0, will be fully dissolved at its center and will 
        /// appear gradually more opaque toward its edge.
        /// </summary>
        public bool Halo { get; set; }
    }
}