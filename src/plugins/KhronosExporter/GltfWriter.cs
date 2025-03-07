using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using ZoDream.KhronosExporter.Converters;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Interfaces;

namespace ZoDream.KhronosExporter
{
    public class GltfWriter : IEntryWriter<ModelRoot>
    {
        public async Task WriteAsync(IStorageFileEntry entry, ModelRoot data)
        {
            if (data is ModelSource s)
            {
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
            var options = new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                Converters =
                {
                    new PropertyPathConverter()
                }
                //WriteIndented = true,
            };
            JsonSerializer.Serialize(output, model, options);
        }

    }
}
