using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    internal class JsonExporter(int entryId, ISerializedFile resource) : IBundleExporter
    {
        internal static readonly JsonSerializerOptions Options = new()
        {
            WriteIndented = true,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Converters =
                {
                    new JsonStringEnumConverter()
                },
        };

        public string FileName => resource[entryId].Name;

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            if (!LocationStorage.TryCreate(fileName, ".json", mode, out fileName))
            {
                return;
            }
            using var fs = File.Create(fileName);
            JsonSerializer.Serialize(fs, resource[entryId], Options);
        }


        public static Utf8JsonWriter OpenWrite(string fileName)
        {
            return new(File.Create(fileName), new JsonWriterOptions()
            {
                Indented = Options.WriteIndented,
                IndentCharacter = Options.IndentCharacter,
                IndentSize = Options.IndentSize,
            });
        }

        public static void Serialize(Utf8JsonWriter writer, object? data)
        {
            JsonSerializer.Serialize(writer, data, Options);
        }
        public static string Serialize(object? data)
        {
            return JsonSerializer.Serialize(data, Options);
        }
    }
}
