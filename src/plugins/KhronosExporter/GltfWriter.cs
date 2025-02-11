using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Interfaces;

namespace ZoDream.KhronosExporter
{
    public class GltfWriter : IEntryWriter<ModelRoot>
    {
        public Task WriteAsync(IStorageFileEntry entry, ModelRoot data)
        {
            throw new NotImplementedException();
        }
        public void Write(ModelRoot model, Stream output)
        {
            var options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                //WriteIndented = true,
            };
            JsonSerializer.Serialize(output, model, options);
        }

    }
}
