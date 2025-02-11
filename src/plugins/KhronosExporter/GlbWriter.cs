using System;
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
        public Task WriteAsync(IStorageFileEntry entry, ModelRoot data)
        {
            throw new NotImplementedException();
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
            var binLengthPos = output.Position;
            writer.Write(0U);
            writer.Write(GlbReader.CHUNKBIN);
            var binPos = output.Position;
            // writer.Write(bin);
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
        }

        private static int GetPaddingLength(long length)
        {
            var padding = (int)(length & 3);
            return padding == 0 ? 0 : (4 - padding);
        } 
    }
}
