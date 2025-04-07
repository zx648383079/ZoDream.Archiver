using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using ZoDream.ShaderDecompiler.SpirV;
using ZoDream.Shared.IO;
using ZoDream.Shared.Language;

namespace ZoDream.ShaderDecompiler
{
    /// <summary>
    /// see https://github.com/KhronosGroup/SPIRV-Headers
    /// </summary>
    public partial class SpirVReader : ILanguageReader
    {

        public SpvBytecode ReadBytecode(Stream input)
        {
            var buffer = new byte[4];
            input.ReadExactly(buffer, 0, 4);
            if (BitConverter.ToUInt32(buffer) == Signature)
            {
                return ReadBytecode(new BinaryReader(input));
            }
            if (BinaryPrimitives.ReadUInt32BigEndian(buffer) == Signature)
            {
                return ReadBytecode(new EndianReader(input, Shared.Models.EndianType.BigEndian));
            }
            throw new Exception("Invalid magic number");
        }

        /// <summary>
        /// 需要手动跳过 magic 文件头
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public SpvBytecode ReadBytecode(BinaryReader input)
        {
            var data = new SpvBytecode();
            data.Header.Version = input.ReadUInt32();
            data.Header.Generator = input.ReadUInt32();
            data.Header.Bound = input.ReadUInt32();
            data.Header.Reserved = input.ReadUInt32();

            data.MainChunk.OpcodeItems = [.. ReadOperand(input)];
            return data;
        }

        private IEnumerable<SpvOperandCode> ReadOperand(BinaryReader input)
        {
            while (input.BaseStream.Position < input.BaseStream.Length)
            {
                var instructionStart = input.ReadUInt32();
                var wordCount = (ushort)(instructionStart >> 16);
                var opCode = (int)(instructionStart & 0xFFFF);

                var words = new uint[wordCount];
                words[0] = instructionStart;
                for (ushort i = 1; i < wordCount; ++i)
                {
                    words[i] = input.ReadUInt32();
                }
                yield return new SpvOperandCode((SpvOperand)opCode, words);
            }
        }
    }
}
