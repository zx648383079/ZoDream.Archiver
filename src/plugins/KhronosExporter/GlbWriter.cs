using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.KhronosExporter
{
    public class GlbWriter : IEntryWriter<ModelRoot>
    {
        public async Task WriteAsync(IStorageFileEntry entry, ModelRoot data)
        {
            using var fs = await entry.OpenWriteAsync();
            if (data is ModelSource s)
            {
                Write(s, fs);
                return;
            }
            Write(new ModelSource(entry.FullPath)
            {
                ExtensionsUsed = data.ExtensionsRequired,
                ExtensionsRequired = data.ExtensionsRequired,
                Accessors = data.Accessors,
                Animations = data.Animations,
                Asset = data.Asset,
                Buffers = data.Buffers,
                BufferViews = data.BufferViews,
                Cameras = data.Cameras,
                Images = data.Images,
                Materials = data.Materials,
                Meshes = data.Meshes,
                Nodes = data.Nodes,
                Samplers = data.Samplers,
                Scene = data.Scene,
                Scenes = data.Scenes,
                Skins = data.Skins,
                Textures = data.Textures,
            }, fs);
        }
        public void Write(ModelSource data, Stream output)
        {
            if (!data.ResourceItems.TryGetValue(string.Empty, out var bin))
            {
                bin = new MemoryStream();
            }
            var maps = new Dictionary<int, long>();
            for (var i = data.Buffers.Count - 1; i >= 0; i--)
            {
                var item = data.Buffers[i];
                if (string.IsNullOrWhiteSpace(item.Uri))
                {
                    bin.Position = item.ByteLength;
                    continue;
                }
                if (ModelSource.TryDecodeBase64String(item.Uri, out var buffer))
                {
                    maps.Add(i, bin.Position);
                    bin.Write(buffer, 0, item.ByteLength);
                }
                else 
                {
                    maps.Add(i, bin.Position);
                    var src = data.GetStream(item);
                    src.Position = 0;
                    src.CopyTo(bin, (long)item.ByteLength);
                }
                data.Buffers.RemoveAt(i);
            }
            foreach (var item in data.BufferViews)
            {
                if (maps.TryGetValue(item.Buffer, out var offset))
                {
                    item.ByteOffset += (int)offset;
                }
            }
            // 更新图片
            /*
            foreach (var item in data.Images)
            {
                if (string.IsNullOrWhiteSpace(item.Uri))
                {
                    continue;
                }
                if (ModelSource.TryDecodeBase64String(item.Uri, out var buffer))
                {
                    item.BufferView = data.BufferViews.AddWithIndex(new()
                    {
                        Name = item.Name,
                        ByteOffset = (int)bin.Position,
                        ByteLength = buffer.Length,
                        Buffer = 0
                    });
                    bin.Write(buffer, 0, buffer.Length);
                    item.Uri = string.Empty;
                }
                else
                {
                    using var src = data.OpenStream(item.Uri);
                    if (src is not null)
                    {
                        item.BufferView = data.BufferViews.AddWithIndex(new()
                        {
                            Name = item.Name,
                            ByteOffset = (int)bin.Position,
                            ByteLength = (int)src.Length,
                            Buffer = 0
                        });
                        src.Position = 0;
                        src.CopyTo(bin);
                        item.Uri = string.Empty;
                    }
                }
            }
            */
            if (data.Buffers.Count == 0)
            {
                data.Buffers.Add(new()
                {
                    Name = "bin"
                });
            }
            data.Buffers[0].ByteLength = (int)bin.Position;
            data.ResourceItems.TryAdd(string.Empty, bin);


            var writer = new EndianWriter(output, EndianType.LittleEndian);
            writer.Write(GlbReader.GLTFHEADER);
            writer.Write(GlbReader.GLTFVERSION2);
            var lengthPos = output.Position;
            writer.Write(0U);
            var jsonLengthPos = output.Position;
            writer.Write(0U);
            writer.Write(GlbReader.CHUNKJSON);
            var jsonPos = output.Position;
            new GltfWriter().Write(data, output);
            var pos = output.Position;
            var jsonPadding = GetPaddingLength(pos - jsonPos);
            for (int i = 0; i < jsonPadding; i++)
            {
                writer.Write((byte)0x20);
            }
            var jsonLength = (uint)(output.Position - jsonPos);
            var binLengthPos = output.Position;
            writer.Write(0U);
            writer.Write(GlbReader.CHUNKBIN);
            var binPos = output.Position;
            // 写入
            bin.Position = 0;
            bin.CopyTo(output);

            pos = output.Position;
            var binPadding = GetPaddingLength(pos - binPos);
            for (int i = 0; i < binPadding; i++)
            {
                writer.Write((byte)0x0);
            }
            var binLength = (uint)(output.Position - binPos);
            var length = (uint)output.Position;

            output.Position = binLengthPos;
            writer.Write(binLength);

            output.Position = jsonLengthPos;
            writer.Write(jsonLength);

            output.Position = lengthPos;
            writer.Write(length);
            writer.Flush();
        }

        public void Write(ModelRoot data, Stream output)
        {
            var writer = new EndianWriter(output, EndianType.LittleEndian);
            writer.Write(GlbReader.GLTFHEADER);
            writer.Write(GlbReader.GLTFVERSION2);
            var lengthPos = output.Position;
            writer.Write(0U);
            var jsonLengthPos = output.Position;
            writer.Write(0U);
            writer.Write(GlbReader.CHUNKJSON);
            var jsonPos = output.Position;
            new GltfWriter().Write(data, output);
            var pos = output.Position;
            var jsonPadding = GetPaddingLength(pos - jsonPos);
            for (int i = 0; i < jsonPadding; i++)
            {
                writer.Write((byte)0x20);
            }
            var jsonLength = (uint)(output.Position - jsonPos);
         
            var length = (uint)output.Position;

            output.Position = jsonLengthPos;
            writer.Write(jsonLength);

            output.Position = lengthPos;
            writer.Write(length);
            writer.Flush();
        }

        

        private static int GetPaddingLength(long length)
        {
            var padding = (int)(length & 4);
            return padding == 0 ? 0 : (4 - padding);
        } 
    }
}
