using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Interfaces;

namespace ZoDream.KhronosExporter
{
    public partial class GltfReader : IEntryReader<ModelRoot>
    {
        public async Task<ModelRoot?> ReadAsync(IStorageFileEntry entry)
        {
            var res = Read(await entry.OpenReadAsync());
            if (res is not null)
            {
                res.FileName = entry.FullPath;
            }
            return res;
        }
        public ModelSource? Read(Stream input)
        {
            return JsonSerializer.Deserialize<ModelSource>(input, GltfWriter.Options);
        }
    }
}
