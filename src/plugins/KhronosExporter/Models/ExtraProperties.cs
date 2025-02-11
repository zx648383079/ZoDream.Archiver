using System.Collections.Generic;

namespace ZoDream.KhronosExporter.Models
{
    public class ExtraProperties
    {

        public Dictionary<string, object> Extensions { get; set; } = [];

        public object? Extras { get; set; }
    }
}
