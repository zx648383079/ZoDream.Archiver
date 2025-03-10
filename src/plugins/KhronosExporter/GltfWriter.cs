using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ZoDream.KhronosExporter.Converters;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Interfaces;

namespace ZoDream.KhronosExporter
{
    public class GltfWriter : IEntryWriter<ModelRoot>
    {
        internal static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters =
                {
                    new PropertyPathConverter()
                },
            //WriteIndented = true,
        };

        public async Task WriteAsync(IStorageFileEntry entry, ModelRoot data)
        {
            if (data is ModelSource s)
            {
                s.FlushBuffer();
                foreach (var item in s.Buffers)
                {
                    if (!string.IsNullOrWhiteSpace(item.Uri))
                    {
                        continue;
                    }
                    var name = $"{entry.Name}_{item.Name}.bin";
                    using var bin = await entry.CreateBrotherAsync(name);
                    s.GetStream(item).CopyTo(bin);
                    item.Uri = name;
                }
                s.ResourceClear();
            }
            using var fs = await entry.OpenWriteAsync();
            Write(data, fs);
        }


        /// <summary>
        /// 把数据写入流中，不处理 Buffers 数据
        /// </summary>
        /// <param name="model"></param>
        /// <param name="output"></param>
        public void Write(ModelRoot model, Stream output)
        {
            model.Asset ??= new()
                {
                    Generator = "Khronos glTF",
                    Version = "2.0"
                };
            for (int i = model.Scenes.Count - 1; i > 0; i--)
            {
                if (model.Scenes[i].Nodes.Count == 0)
                {
                    model.Scenes.RemoveAt(i);
                }
            }
            JsonSerializer.Serialize(output, model, Options);
        }

    }
}
