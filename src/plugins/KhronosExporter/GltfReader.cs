using System.IO;
using System.Text.Json;
using ZoDream.KhronosExporter.Models;

namespace ZoDream.KhronosExporter
{
    public class GltfReader
    {
        public ModelRoot? Read(Stream input)
        {
            using var doc = JsonDocument.Parse(input);
            var root = doc.RootElement;
            var data = new ModelRoot();

        }
    }
}
