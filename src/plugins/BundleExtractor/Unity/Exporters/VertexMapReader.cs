using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using ZoDream.Shared;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    /// <summary>
    /// COLOR_0 vec4
    /// </summary>
    public class VertexMapReader
    {
        public object Read(Stream input)
        {
            var reader = new BinaryReader(input);
            Expectation.ThrowIfNot(reader.ReadByte() == 0xA);
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
                Expectation.ThrowIfNot(signature is 0xA or 0x12);
                var len = reader.Read7BitEncodedInt();
                nameItems.Add(Encoding.ASCII.GetString(reader.ReadBytes(len)));
            }
            while(signature == 0x1A && input.Position < input.Length)
            {
                var len = reader.Read7BitEncodedInt();
                var end = reader.BaseStream.Position + len;
                while (reader.BaseStream.Position < end)
                {
                    // 第一个值 8 表示 uv 表明 第二 值对应 blend shape 的 index
                    // 第二个值 是自增的 序号 从 0 开始 为 0 时 第一第二值可能没有
                    // 第三个值 16 表示 vec4 表明第四个值 tangent
                    // 第四个值 对应 blend shape index
                    var value = reader.Read7BitEncodedInt();
                }
                if (input.Position >= input.Length)
                {
                    break;
                }
                signature = reader.ReadByte();
            }
            Expectation.ThrowIfNot(reader.BaseStream.Position - start == dataSize);
            return true;
        }

        internal static bool IsSupport(byte[] buffer, int length)
        {
            if (buffer[0] != 0xA)
            {
                return false;
            }
            var i = 1;
            while (buffer[i++] < 0x80 && i < 10)
            {
            }
            return length > i && buffer[i] == 0xA;
        }
    }
}
