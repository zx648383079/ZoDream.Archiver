using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZoDream.BundleExtractor.Unity.UI;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class JsonExporter(UIObject data) : IBundleExporter
    {
        private static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            Converters =
                {
                    new JsonStringEnumConverter()
                },
        };

        public string Name => data.Name;

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".json", mode, out fileName))
            {
                return;
            }
            using var fs = File.Create(fileName);
            JsonSerializer.Serialize(fs, data, Options);
        }
    }
}
