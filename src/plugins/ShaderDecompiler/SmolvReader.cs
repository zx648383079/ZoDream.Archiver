using System;
using System.IO;
using ZoDream.ShaderDecompiler.SpirV;
using ZoDream.Shared.Language;

namespace ZoDream.ShaderDecompiler
{
    public partial class SmolvReader(Stream stream) : ILanguageReader<SpvBytecode>
    {
        
        /// <summary>
        /// 'SMOL' ascii
        /// </summary>
        public const uint Signature = 0x534D4F4C;

        private const int HeaderSize = 6 * sizeof(uint);

        public SpvBytecode Read()
        {
            return ReadBytecode(stream);
        }


        public static bool IsSupport(byte[] buffer, int length)
        {
            return BitConverter.ToUInt32(buffer) == Signature;
        }

    }
}
