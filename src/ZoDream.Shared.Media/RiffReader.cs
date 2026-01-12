using System.Diagnostics;
using System.IO;
using System.Text;
using ZoDream.Shared.Media.Models;

namespace ZoDream.Shared.Media
{
    public class RiffReader
    {


        public RiffChunk Read(Stream input)
        {
            var pos = input.Position;
            var reader = new BinaryReader(input);
            var magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            Expectation.ThrowIfNotSignature("RIFF", magic);
            input.Position = pos;
            return (RiffChunk)Read(reader);
        }

        private IRiffChunk Read(BinaryReader reader)
        {
            var magic = Encoding.ASCII.GetString(reader.ReadBytes(4));
            var length = reader.ReadUInt32();
            if (magic != "RIFF" && magic != "LIST")
            {
                var chunk = new RiffSubChunk()
                {
                    Id = magic,
                    Length = length,
                    Position = reader.BaseStream.Position
                };
                reader.BaseStream.Seek(chunk.Length, SeekOrigin.Current);
                return chunk;
            }
            {
                var chunk = new RiffChunk()
                {
                    Id = magic,
                    Length = length,
                    Type = Encoding.ASCII.GetString(reader.ReadBytes(4)),
                };
                var end = reader.BaseStream.Position + length;
                while (reader.BaseStream.Position < end)
                {
                    chunk.Items.Add(Read(reader));
                }
                return chunk;
            }
        }
    }
}
