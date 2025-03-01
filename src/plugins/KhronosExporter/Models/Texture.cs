using System.Text.Json.Serialization;
using ZoDream.KhronosExporter.Converters;

namespace ZoDream.KhronosExporter.Models
{
    public class Texture : LogicalChildOfRoot
    {
        [JsonConverter(typeof(IndexConverter))]
        public int Sampler { get; set; } = -1;

        [JsonConverter(typeof(IndexConverter))]
        public int Source { get; set; } = -1;

    }
}
