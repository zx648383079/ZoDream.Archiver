namespace ZoDream.KhronosExporter.Models
{
    public class AccessorSparseIndices : ExtraProperties
    {

        public int BufferView {  get; set; }

        public int ByteOffset { get; set; }

        public EncodingType ComponentType { get; set; }
    }
}
