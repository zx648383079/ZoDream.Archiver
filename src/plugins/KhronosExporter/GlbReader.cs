using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ZoDream.KhronosExporter.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.KhronosExporter
{
    public class GlbReader : IEntryReader<ModelRoot>
    {
        public const uint GLTFHEADER = 0x46546C67;
        public const uint GLTFVERSION2 = 2;
        public const uint CHUNKJSON = 0x4E4F534A;
        public const uint CHUNKBIN = 0x004E4942;
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
            var reader = new BundleBinaryReader(input, EndianType.LittleEndian);
            var beginPosition = reader.Position;
            var magic = reader.ReadUInt32();
            Debug.Assert(magic == GLTFHEADER);
            var version = reader.ReadUInt32();
            Debug.Assert(version == GLTFVERSION2);
            var bodyLength = reader.ReadUInt32();
            var chunkItems = new Dictionary<uint, Stream>();
            while (reader.Position - beginPosition < bodyLength)
            {
                var chunkLength = reader.ReadUInt32();
                Debug.Assert(chunkLength > 0 && chunkLength % 4 == 0);
                var chunkId = reader.ReadUInt32();
                chunkItems.TryAdd(chunkId, new PartialStream(
                    reader.BaseStream,
                    chunkLength
                ));
                reader.BaseStream.Seek(chunkLength, SeekOrigin.Current);
            }
            if (!chunkItems.TryGetValue(CHUNKJSON, out var stream))
            {
                return null;
            }
            var res = new GltfReader().Read(stream);
            if (res is not null && chunkItems.TryGetValue(CHUNKBIN, out var bin))
            {
                res.ResourceItems.Add(string.Empty, bin);
            }
            return res;
        }

     
    }
}
