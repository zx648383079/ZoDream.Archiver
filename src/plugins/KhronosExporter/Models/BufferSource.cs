namespace ZoDream.KhronosExporter.Models
{
    public class BufferSource : LogicalChildOfRoot
    {
        /// <summary>
        /// 当为空是是自带的
        /// </summary>
        public string Uri { get; set; }

        public int ByteLength { get; set; }

    }
}
