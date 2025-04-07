using System;
using System.IO;
using ZoDream.Shared.Language;
using ZoDream.Shared.Language.AST;

namespace ZoDream.ShaderDecompiler
{
    public partial class SmolvReader : ILanguageReader
    {
        
        /// <summary>
        /// 'SMOL' ascii
        /// </summary>
        public const uint Signature = 0x534D4F4C;

        private const int HeaderSize = 6 * sizeof(uint);

        public GlobalExpression Read(Stream input)
        {
            var builder = new GlobalExpression();
            return builder;
        }


        public static bool IsSupport(byte[] buffer, int length)
        {
            return BitConverter.ToUInt32(buffer) == Signature;
        }
    }
}
