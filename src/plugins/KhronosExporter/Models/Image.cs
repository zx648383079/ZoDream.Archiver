using System.Text.Json.Serialization;
using ZoDream.KhronosExporter.Converters;

namespace ZoDream.KhronosExporter.Models
{
    public class Image : LogicalChildOfRoot
    {
        /// <summary>
        /// data:image/png;base64,
        /// </summary>
        public string Uri { get; set; }

        public string MimeType { get; set; }

        [JsonConverter(typeof(IndexConverter))]
        public int BufferView { get; set; } = -1;

    }
}
