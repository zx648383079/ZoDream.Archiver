using System.Text.Json.Serialization;
using ZoDream.KhronosExporter.Converters;

namespace ZoDream.KhronosExporter.Models
{
    public class BufferView : LogicalChildOfRoot
    {
        /// <summary>
        /// 指向 bufferSource
        /// </summary>
        [JsonConverter(typeof(IndexConverter))]
        public int Buffer { get; set; } = -1;

        public int ByteOffset { get; set; }

        public int ByteLength { get; set; }

        public int ByteStride { get; set; }

        public BufferMode Target { get; set; }

    }
}
