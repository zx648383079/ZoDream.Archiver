namespace ZoDream.KhronosExporter.Models
{
    public class BufferView : LogicalChildOfRoot
    {
        /// <summary>
        /// 指向 bufferSource
        /// </summary>
        public int Buffer {  get; set; }

        public int ByteOffset { get; set; }

        public int ByteLength { get; set; }

        public int ByteStride { get; set; }

        public BufferMode Target { get; set; }

    }
}
