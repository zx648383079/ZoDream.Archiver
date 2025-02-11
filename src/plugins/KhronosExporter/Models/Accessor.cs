namespace ZoDream.KhronosExporter.Models
{
    public class Accessor : LogicalChildOfRoot
    {

        public int BufferView {  get; set; }

        public int ByteOffset { get; set; }

        public EncodingType ComponentType { get; set; }

        public bool Normalized { get; set; }

        public int Count { get; set; }

        /// <summary>
        /// SCALAR|VEC2|VEC3|VEC4|MAT2|MAT3|MAT4
        /// </summary>
        public string Type { get; set; }

        public float[] Max {  get; set; }

        public float[] Min { get; set; }

        public AccessorSparse? Sparse { get; set; }

    }
}
