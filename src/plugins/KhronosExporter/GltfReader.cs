using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ZoDream.KhronosExporter.Converters;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Interfaces;

namespace ZoDream.KhronosExporter
{
    public partial class GltfReader : IEntryReader<ModelRoot>
    {
        public Task<ModelRoot?> ReadAsync(IStorageFileEntry entry)
        {
            throw new System.NotImplementedException();
        }
        public ModelRoot? Read(Stream input)
        {
            var options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new PropertyPathConverter()
                }
            };
            return JsonSerializer.Deserialize<ModelRoot>(input, options);
        }
    }
}
