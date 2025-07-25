﻿using System.IO;
using System.Text;
using ZoDream.Shared;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Exporters
{
    /// <summary>
    /// 导出Blend Shapes作为Morph Targets
    /// </summary>
    public class BlendShapeReader
    {
        public object Read(Stream input)
        {
            var reader = new BinaryReader(input);
            var signature = reader.ReadByte();
            Expectation.ThrowIfNot(signature == 0xA);
            var dataSize = reader.Read7BitEncodedInt();
            var start = reader.BaseStream.Position;
            Expectation.ThrowIfNot(reader.ReadByte() == 0xA);
            var len = reader.Read7BitEncodedInt();
            var name = Encoding.ASCII.GetString(reader.ReadBytes(len));
            while (true)
            {
                ReadPart(reader);
                if (reader.ReadByte() == 0x18)
                {
                    Expectation.ThrowIfNot(reader.ReadBytes(3).Equal([0xC0, 0xE3, 0x1]));
                    break;
                }
                reader.BaseStream.Seek(-1, SeekOrigin.Current);
            }
            Expectation.ThrowIfNot(reader.BaseStream.Position - start == dataSize);
            return true;
        }

        private void ReadPart(BinaryReader reader)
        {
            var signature = reader.ReadByte();
            Expectation.ThrowIfNot(signature is 0xA or 0x12);
            var dataSize = reader.Read7BitEncodedInt();
            var start = reader.BaseStream.Position;
            Expectation.ThrowIfNot(reader.ReadByte() == 0xA);
            ReadChild(reader);
            Expectation.ThrowIfNot(reader.BaseStream.Position - start == dataSize);
        }

        private void ReadChild(BinaryReader reader)
        {
            var len = reader.Read7BitEncodedInt();
            var name = Encoding.ASCII.GetString(reader.ReadBytes(len));
            Expectation.ThrowIfNot(reader.ReadByte() == 0x12);
            
            var dataSize = reader.Read7BitEncodedInt();
            var start = reader.BaseStream.Position;
            var signature = reader.ReadByte();
            if (signature != 0xD)
            {
                ReadChild(reader);
                Expectation.ThrowIfNot(reader.BaseStream.Position - start == dataSize);
                return;
            }
            Expectation.ThrowIfNot(reader.ReadBytes(4).Equal([0x0, 0x0, 0xC8, 0x42]));
            while (true)
            {
                var pos = reader.BaseStream.Position;
                signature = reader.ReadByte();
                if (signature != 0x12)
                {
                    reader.BaseStream.Seek(pos, SeekOrigin.Begin);
                    break;
                }
                len = reader.Read7BitEncodedInt();
                var s = reader.BaseStream.Position;
                if (reader.ReadByte() == 0xA)
                {
                    reader.BaseStream.Seek(pos, SeekOrigin.Begin);
                    break;
                }
                var index = reader.Read7BitEncodedInt();
                // 对应 vertex map 的 第四个值
                while (true)
                {
                    if (reader.ReadByte() is not 0x1D and not 0x25 and not 0x2D)
                    {
                        reader.BaseStream.Seek(-1, SeekOrigin.Current);
                        break;
                    }
                    var val = reader.ReadSingle();
                }
                Expectation.ThrowIfNot(reader.BaseStream.Position - s == len);
            }
            Expectation.ThrowIfNot(reader.BaseStream.Position - start == dataSize);
        }

        internal static bool IsSupport(Stream input)
        {
            input.Seek(- 4, SeekOrigin.End);
            var buffer = input.ReadBytes(4);
            input.Position = 0;
            return buffer.Equal([0x18, 0xC0, 0xE3, 0x1]);
        }
    }
}
