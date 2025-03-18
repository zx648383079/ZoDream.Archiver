namespace ZoDream.KhronosExporter.Models
{
    public class BufferView : LogicalChildOfRoot
    {
        public int Buffer { get; set; } = -1;

        public int ByteOffset { get; set; }

        public int ByteLength { get; set; }
        /// <summary>
        /// 只有 vec 可以定义这个
        /// </summary>
        public int? ByteStride { get; set; }

        public BufferMode? Target { get; set; }

    }
}
