using System;
using System.Buffers;
using System.Diagnostics;
using System.IO;
using System.Text;
using ZoDream.ShaderDecompiler.Smolv;
using ZoDream.Shared.Language;
using ZoDream.Shared.Language.AST;

namespace ZoDream.ShaderDecompiler
{
    public class SmolvReader : ILanguageReader
    {
        
        /// <summary>
        /// 'SMOL' ascii
        /// </summary>
        public const uint HeaderMagic = 0x534D4F4C;

        private const int HeaderSize = 6 * sizeof(uint);

        public GlobalExpression Read(Stream input)
        {
            var builder = new GlobalExpression();
            return builder;
        }

        public string ReadToString(Stream input)
        {
            if (input.Position + HeaderSize > input.Length)
            {
                throw new ArgumentException(nameof(input));
            }
            var reader = new BinaryReader(input, Encoding.UTF8);
            var magic = reader.ReadUInt32(); // HeaderMagic
            Debug.Assert(magic == HeaderMagic);
            var version = reader.ReadUInt32();
            var generator = reader.ReadUInt32();
            var bound = reader.ReadUInt32();
            var schema = reader.ReadUInt32();
            var decodedSize = (int)reader.ReadUInt32();
            if (decodedSize == 0)
            {
                throw new Exception("Invalid SMOL-V shader header");
            }
            var buffer = ArrayPool<byte>.Shared.Rent(decodedSize);
            try
            {
                var output = new MemoryStream(buffer, 0, decodedSize);
                var writer = new BinaryWriter(output, Encoding.UTF8);
                writer.Write(SpirVReader.HeaderMagic);
                writer.Write(version);
                writer.Write(generator);
                writer.Write(bound);
                writer.Write(schema);
                if (!Convert(reader, writer) || writer.BaseStream.Position != decodedSize)
                {
                    throw new Exception("Unable to decode SMOL-V shader");
                }
                output.Position = 4;
                return new SpirVReader().ReadToString(new BinaryReader(output));
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private bool Convert(BinaryReader input, BinaryWriter output)
        {
            int prevResult = 0;
            int prevDecorate = 0;
            while (input.BaseStream.Position < input.BaseStream.Length)
            {
                // read length + opcode
                if (!ReadLengthOp(input, out uint instrLen, out SmvOperand op))
                {
                    return false;
                }

                bool wasSwizzle = op == SmvOperand.VectorShuffleCompact;
                if (wasSwizzle)
                {
                    op = SmvOperand.VectorShuffle;
                }
                output.Write((instrLen << 16) | (uint)op);

                uint ioffs = 1;
                // read type as varint, if we have it
                var opCode = SmvOperandCode.Get(op);
                if (opCode.HasType != 0)
                {
                    if (!ReadVarint(input, out uint value))
                    {
                        return false;
                    }

                    output.Write(value);
                    ioffs++;
                }

                // read result as delta + varint, if we have it
                if (opCode.HasResult != 0)
                {
                    if (!ReadVarint(input, out uint value))
                    {
                        return false;
                    }

                    int zds = prevResult + ZigDecode(value);
                    output.Write(zds);
                    prevResult = zds;
                    ioffs++;
                }

                // Decorate: IDs relative to previous decorate
                if (op == SmvOperand.Decorate || op == SmvOperand.MemberDecorate)
                {
                    if (!ReadVarint(input, out uint value))
                    {
                        return false;
                    }

                    int zds = prevDecorate + unchecked((int)value);
                    output.Write(zds);
                    prevDecorate = zds;
                    ioffs++;
                }

                // Read this many IDs, that are relative to result ID
                int relativeCount = opCode.DeltaFromResult;
                bool inverted = false;
                if (relativeCount < 0)
                {
                    inverted = true;
                    relativeCount = -relativeCount;
                }
                for (int i = 0; i < relativeCount && ioffs < instrLen; ++i, ++ioffs)
                {
                    if (!ReadVarint(input, out uint value))
                    {
                        return false;
                    }

                    int zd = inverted ? ZigDecode(value) : unchecked((int)value);
                    output.Write(prevResult - zd);
                }

                if (wasSwizzle && instrLen <= 9)
                {
                    uint swizzle = input.ReadByte();
                    if (instrLen > 5)
                    {
                        output.Write(swizzle >> 6);
                    }
                    if (instrLen > 6)
                    {
                        output.Write((swizzle >> 4) & 3);
                    }
                    if (instrLen > 7)
                    {
                        output.Write((swizzle >> 2) & 3);
                    }
                    if (instrLen > 8)
                    {
                        output.Write(swizzle & 3);
                    }
                }
                else if (opCode.VarRest != 0)
                {
                    // read rest of words with variable encoding
                    for (; ioffs < instrLen; ++ioffs)
                    {
                        if (!ReadVarint(input, out uint value))
                        {
                            return false;
                        }
                        output.Write(value);
                    }
                }
                else
                {
                    // read rest of words without any encoding
                    for (; ioffs < instrLen; ++ioffs)
                    {
                        if (input.BaseStream.Position + 4 > input.BaseStream.Length)
                        {
                            return false;
                        }
                        var val = input.ReadUInt32();
                        output.Write(val);
                    }
                }
            }
            return true;
        }

        private static bool ReadVarint(BinaryReader input, out uint value)
        {
            uint v = 0;
            int shift = 0;
            while (input.BaseStream.Position < input.BaseStream.Length)
            {
                byte b = input.ReadByte();
                v |= unchecked((uint)(b & 127) << shift);
                shift += 7;
                if ((b & 128) == 0)
                {
                    break;
                }
            }

            value = v;
            // @TODO: report failures
            return true;
        }

        private static bool ReadLengthOp(BinaryReader input, out uint len, out SmvOperand op)
        {
            len = default;
            op = default;
            if (!ReadVarint(input, out uint value))
            {
                return false;
            }
            len = ((value >> 20) << 4) | ((value >> 4) & 0xF);
            op = (SmvOperand)(((value >> 4) & 0xFFF0) | (value & 0xF));

            op = RemapOp(op);
            len = DecodeLen(op, len);
            return true;
        }

        /// <summary>
        /// Remap most common Op codes (Load, Store, Decorate, VectorShuffle etc.) to be in &lt; 16 range, for 
        /// more compact varint encoding. This basically swaps rarely used op values that are &lt; 16 with the
        /// ones that are common.
        /// </summary>
        private static SmvOperand RemapOp(SmvOperand op)
        {
            return op switch
            {
                // 0: 24%
                SmvOperand.Decorate => SmvOperand.Nop,
                SmvOperand.Nop => SmvOperand.Decorate,
                // 1: 17%
                SmvOperand.Load => SmvOperand.Undef,
                SmvOperand.Undef => SmvOperand.Load,
                // 2: 9%
                SmvOperand.Store => SmvOperand.SourceContinued,
                SmvOperand.SourceContinued => SmvOperand.Store,
                // 3: 7.2%
                SmvOperand.AccessChain => SmvOperand.Source,
                SmvOperand.Source => SmvOperand.AccessChain,
                // 4: 5.0%
                // Name - already small enum value - 5: 4.4%
                // MemberName - already small enum value - 6: 2.9% 
                SmvOperand.VectorShuffle => SmvOperand.SourceExtension,
                SmvOperand.SourceExtension => SmvOperand.VectorShuffle,
                // 7: 4.0%
                SmvOperand.MemberDecorate => SmvOperand.String,
                SmvOperand.String => SmvOperand.MemberDecorate,
                // 8: 0.9%
                SmvOperand.Label => SmvOperand.Line,
                SmvOperand.Line => SmvOperand.Label,
                // 9: 3.9%
                SmvOperand.Variable => (SmvOperand)9,
                (SmvOperand)9 => SmvOperand.Variable,
                // 10: 3.9%
                SmvOperand.FMul => SmvOperand.Extension,
                SmvOperand.Extension => SmvOperand.FMul,
                // 11: 2.5%
                // ExtInst - already small enum value - 12: 1.2%
                // VectorShuffleCompact - already small enum value - used for compact shuffle encoding
                SmvOperand.FAdd => SmvOperand.ExtInstImport,
                SmvOperand.ExtInstImport => SmvOperand.FAdd,
                // 14: 2.2%
                SmvOperand.TypePointer => SmvOperand.MemoryModel,
                SmvOperand.MemoryModel => SmvOperand.TypePointer,
                // 15: 1.1%
                SmvOperand.FNegate => SmvOperand.EntryPoint,
                SmvOperand.EntryPoint => SmvOperand.FNegate,
                _ => op,
            };
        }

        private static uint DecodeLen(SmvOperand op, uint len)
        {
            len++;
            switch (op)
            {
                case SmvOperand.VectorShuffle:
                    len += 4;
                    break;
                case SmvOperand.VectorShuffleCompact:
                    len += 4;
                    break;
                case SmvOperand.Decorate:
                    len += 2;
                    break;
                case SmvOperand.Load:
                    len += 3;
                    break;
                case SmvOperand.AccessChain:
                    len += 3;
                    break;
            }
            return len;
        }

        private static int DecorationExtraOps(int dec)
        {
            // RelaxedPrecision, Block..ColMajor
            if (dec == 0 || (dec >= 2 && dec <= 5))
            {
                return 0;
            }
            // Stream..XfbStride
            if (dec >= 29 && dec <= 37)
            {
                return 1;
            }

            // unknown, encode length
            return -1;
        }
        private static int ZigDecode(uint u)
        {
            return (u & 1) != 0 ? unchecked((int)(~(u >> 1))) : unchecked((int)(u >> 1));
        }
    }
}
