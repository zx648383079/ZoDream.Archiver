using System.IO;
using System.Numerics;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ZoDream.KhronosExporter.Converters;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;

namespace ZoDream.KhronosExporter
{
    public class GltfWriter : IEntryWriter<ModelRoot>
    {
        internal static readonly JsonSerializerOptions Options = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals,
            Converters =
                {
                    new EnumNameConverter<PropertyPath>(),
                    new EnumNameConverter<AnimationInterpolationMode>(),
                    new EnumNameConverter<LightType>(),
                    new VectorConverter<Vector2>(),
                    new VectorConverter<Vector3>(),
                    new VectorConverter<Vector4>(),
                    new VectorConverter<Quaternion>(),
                    new VectorConverter<Matrix4x4>(),
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
                    var ms = s.GetStream(item);
                    ms.Position = 0;
                    ms.CopyTo(bin);
                    item.Uri = name;
                }
                s.ResourceClear();
            }
            using var fs = await entry.OpenWriteAsync();
            Serialize(data, fs);
        }

        /// <summary>
        /// 所有 Buffers 数据 转成 base64 
        /// </summary>
        /// <param name="model"></param>
        /// <param name="output"></param>
        public void Write(ModelRoot data, Stream output)
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
                    var ms = s.GetStream(item);
                    ms.Position = 0;
                    item.Uri = ms.ToBase64String("application/gltf-buffer");
                }
                s.ResourceClear();
            }
            Serialize(data, output);
        }

        /// <summary>
        /// 把数据写入流中，不处理 Buffers 数据
        /// </summary>
        /// <param name="model"></param>
        /// <param name="output"></param>
        public void Serialize(ModelRoot model, Stream output)
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
