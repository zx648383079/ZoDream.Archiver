using System.Collections.Generic;
using System.Text.Json.Serialization;
using ZoDream.KhronosExporter.Converters;

namespace ZoDream.KhronosExporter.Models
{
    public class ExtraProperties
    {
        [JsonConverter(typeof(ExtensionConverter))]
        public Dictionary<string, object>? Extensions { get; set; }

        public object? Extras { get; set; }

        public void AddExtension(string key, object data)
        {
            Extensions ??= [];
            if (!Extensions.TryAdd(key, data))
            {
                Extensions[key] = data;
            }
        }
    }
}
