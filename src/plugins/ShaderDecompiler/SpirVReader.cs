using System;
using System.Buffers.Binary;
using System.IO;
using ZoDream.ShaderDecompiler.SpirV;
using ZoDream.Shared.Language;

namespace ZoDream.ShaderDecompiler
{
    /// <summary>
    /// see https://github.com/KhronosGroup/SPIRV-Headers
    /// </summary>
    public partial class SpirVReader(Stream stream) : ILanguageReader<SpvBytecode>
    {
        public const uint Signature = 0x07230203;
        public const uint Version12 = 0x10200;
        public const uint Version11 = 0x10100;
        public const uint Version10 = 0x10000;
        public const uint Revision12 = 2;
        public const uint Revision11 = 8;
        public const uint Revision10 = 12;
        public const uint OpCodeMask = 0xffff;
        public const uint WordCountShift = 16;
        public SpvBytecode Read()
        {
            return ReadBytecode(stream);
        }

        public static bool IsSupport(byte[] buffer, int length)
        {
            return BitConverter.ToUInt32(buffer) == Signature || BinaryPrimitives.ReadUInt32BigEndian(buffer) == Signature;
        }
    }
}
