using System.IO;
using System.Linq;
using System.Text.Json;
using ZoDream.Live2dExporter.Models;

namespace ZoDream.Live2dExporter
{
    internal class MocJsonReader
    {

        public static string[] LoadTexture(string fileName)
        {
            var folder = Path.GetDirectoryName(fileName);
            var data = JsonSerializer.Deserialize<JsonModelRoot>(File.ReadAllText(fileName));
            if (data is null)
            {
                return [];
            }
            return data.FileReferences.Textures.Select(i => Path.Combine(folder, i)).ToArray();
        }

        public JsonModelRoot? Deserialize(string content)
        {
            return JsonSerializer.Deserialize<JsonModelRoot>(content);
        }

        public JsonModelRoot? Deserialize(Stream input)
        {
            return JsonSerializer.Deserialize<JsonModelRoot>(input);
        }

        public string Serialize(JsonModelRoot data)
        {
            return JsonSerializer.Serialize(data);
        }

        public void Serialize(JsonModelRoot data, Stream output)
        {
            JsonSerializer.Serialize(output, data);
        }
    }
}
