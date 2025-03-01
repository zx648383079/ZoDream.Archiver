namespace ZoDream.KhronosExporter.Models
{
    public class AccessorSparse : ExtraProperties
    {
        public int Count { get; set; }

        public AccessorSparseIndices Indices { get; set; }

        public AccessorSparseValues Values { get; set; }
    }
}
