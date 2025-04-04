using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    public class VertexMapReader
    {
        public object Read(Stream input)
        {
            var reader = new BinaryReader(input);
            Debug.Assert(reader.ReadByte() == 0xA);
            var dataSize = reader.Read7BitEncodedInt();
            var start = reader.BaseStream.Position;
            var nameItems = new List<string>();
            byte signature;
            while (true)
            {
                signature = reader.ReadByte();
                if (signature == 0x1A)
                {
                    break;
                }
                Debug.Assert(signature is 0xA or 0x12);
                var len = reader.Read7BitEncodedInt();
                nameItems.Add(Encoding.ASCII.GetString(reader.ReadBytes(len)));
            }
            while(signature == 0x1A && input.Position < input.Length)
            {
                var len = reader.Read7BitEncodedInt();
                var end = reader.BaseStream.Position + len;
                while (reader.BaseStream.Position < end)
                {
                    var value = reader.Read7BitEncodedInt();
                }
                if (input.Position >= input.Length)
                {
                    break;
                }
                signature = reader.ReadByte();
            }
            Debug.Assert(reader.BaseStream.Position - start == dataSize);
            return true;
        }

        internal static bool IsSupport(byte[] buffer, int length)
        {
            if (buffer[0] != 0xA)
            {
                return false;
            }
            var i = 1;
            while (buffer[i++] < 0x80)
            {
            }
            return length > i && buffer[i] == 0xA;
        }
    }
}
