using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ZoDream.ShaderDecompiler.Smolv;
using ZoDream.ShaderDecompiler.SpirV;

namespace ZoDream.ShaderDecompiler
{
    public partial class SmolvReader
    {
        public SpvBytecode ReadBytecode(Stream input)
        {
            if (input.Position + HeaderSize > input.Length)
            {
                throw new ArgumentException(nameof(input));
            }
            var reader = new BinaryReader(input, Encoding.UTF8);
            var magic = reader.ReadUInt32(); // HeaderMagic
            Debug.Assert(magic == Signature);
            var data = new SpvBytecode();
            var version = reader.ReadUInt32();
            var smolVersion = version >> 24;
            data.Header.Version = version & 0x00FFFFFF;
            data.Header.Generator = reader.ReadUInt32();
            data.Header.Bound = reader.ReadUInt32();
            data.Header.Reserved = reader.ReadUInt32();
            var decodedSize = (int)reader.ReadUInt32();
            if (decodedSize == 0)
            {
                throw new Exception("Invalid SMOL-V shader header");
            }
            data.MainChunk.OpcodeItems = ReadOperand(reader, (int)smolVersion).ToArray();
            return data;
        }

        private static void ThrowDecodeException()
        {
            throw new Exception("Unable to decode SMOL-V shader");
        }

        private IEnumerable<SpvOperandCode> ReadOperand(BinaryReader input, int version)
        {
            var beforeZeroVersion = version == 0;
            var knownOpsCount = version switch
            {
                0 => (int)SmvOperand.ModuleProcessed + 1,
                1 => (int)SmvOperand.GroupNonUniformQuadSwap + 1,
                _ => 0,
            };
            int prevResult = 0;
            int prevDecorate = 0;
            uint val;
            while (input.BaseStream.Position < input.BaseStream.Length)
            {
                // read length + opcode
                if (!ReadLengthOp(input, out uint instrLen, out SmvOperand op))
                {
                    ThrowDecodeException();
                }
                var words = new List<uint>((int)instrLen);
                bool wasSwizzle = op == SmvOperand.VectorShuffleCompact;
                if (wasSwizzle)
                {
                    op = SmvOperand.VectorShuffle;
                }
                words.Add((instrLen << 16) | (uint)op);

                uint ioffs = 1;
                var isVerify = op >= 0 && (int)op < knownOpsCount;
                // read type as varint, if we have it
                var opCode = SmvOperandCode.Get(op);
                if (isVerify && opCode.HasType != 0)
                {
                    if (!ReadVarint(input, out val))
                    {
                        ThrowDecodeException();
                    }
                    words.Add(val);
                    ioffs++;
                }

                // read result as delta + varint, if we have it
                if (isVerify && opCode.HasResult != 0)
                {
                    if (!ReadVarint(input, out val))
                    {
                        ThrowDecodeException();
                    }

                    int zds = prevResult + ZigDecode(val);
                    prevResult = zds;
                    words.Add((uint)zds);
                    ioffs++;
                }

                // Decorate: IDs relative to previous decorate
                if (op == SmvOperand.Decorate || op == SmvOperand.MemberDecorate)
                {
                    if (!ReadVarint(input, out val))
                    {
                        ThrowDecodeException();
                    }

                    int zds = prevDecorate + (beforeZeroVersion ? (int)val : ZigDecode(val));
                    prevDecorate = zds;
                    words.Add((uint)zds);
                    ioffs++;
                }

                if (op == SmvOperand.MemberDecorate && !beforeZeroVersion)
                {
                    if (input.BaseStream.Position >= input.BaseStream.Length)
                    {
                        ThrowDecodeException();
                    }
                    int count = input.ReadByte();
                    int prevIndex = 0;
                    int prevOffset = 0;
                    for (int m = 0; m < count; ++m)
                    {
                        // read member index
                        if (!ReadVarint(input, out var memberIndex))
                        {
                            ThrowDecodeException();
                        }
                        memberIndex += (uint)prevIndex;
                        prevIndex = (int)memberIndex;

                        // decoration (and length if not common/known)
                        if (!ReadVarint(input, out var memberDec))
                        {
                            ThrowDecodeException();
                        }
                        var knownExtraOps = DecorationExtraOps((int)memberDec);
                        uint memberLen;
                        if (knownExtraOps == -1)
                        {
                            if (!ReadVarint(input, out memberLen))
                            {
                                ThrowDecodeException();
                            }
                            memberLen += 4;
                        }
                        else
                        {
                            memberLen = 4 + (uint)knownExtraOps;
                        }

                        // write SPIR-V op+length (unless it's first member decoration, in which case it was written before)
                        if (m != 0)
                        {
                            words.Add((memberLen << 16) | (uint)op);
                            words.Add((uint)prevDecorate);
                        }
                        words.Add(memberIndex);
                        words.Add(memberDec);
                        // Special case for Offset decorations
                        if (memberDec == 35) // Offset
                        {
                            if (memberLen != 5)
                            {
                                ThrowDecodeException();
                            }
                            if (!ReadVarint(input, out val))
                            {
                                ThrowDecodeException();
                            }
                            val += (uint)prevOffset;
                            words.Add(val);
                            prevOffset = (int)val;
                        }
                        else
                        {
                            for (var i = 4; i < memberLen; ++i)
                            {
                                if (!ReadVarint(input, out val))
                                {
                                    ThrowDecodeException();
                                }
                                words.Add(val);
                            }
                        }
                    }
                    continue;
                }

                // Read this many IDs, that are relative to result ID
                int relativeCount = isVerify ? opCode.DeltaFromResult : 0;
                bool zigDecodeVals = true;
                if (beforeZeroVersion)
                {
                    if (op != SmvOperand.ControlBarrier
                        && op != SmvOperand.MemoryBarrier 
                        && op != SmvOperand.LoopMerge && op != SmvOperand.SelectionMerge 
                        && op != SmvOperand.Branch && op != SmvOperand.BranchConditional 
                        && op != SmvOperand.MemoryNamedBarrier)
                    {
                        zigDecodeVals = false;
                    }
                }
                for (int i = 0; i < relativeCount && ioffs < instrLen; ++i, ++ioffs)
                {
                    if (!ReadVarint(input, out val))
                    {
                        ThrowDecodeException();
                    }
                    if (zigDecodeVals)
                    {
                        val = (uint)ZigDecode(val);
                    }
                    words.Add((uint)prevResult - val);
                }

                if (wasSwizzle && instrLen <= 9)
                {
                    uint swizzle = input.ReadByte();
                    if (instrLen > 5)
                    {
                        words.Add((swizzle >> 6) & 3);
                    }
                    if (instrLen > 6)
                    {
                        words.Add((swizzle >> 4) & 3);
                    }
                    if (instrLen > 7)
                    {
                        words.Add((swizzle >> 2) & 3);
                    }
                    if (instrLen > 8)
                    {
                        words.Add(swizzle & 3);
                    }
                }
                else if (isVerify && opCode.VarRest != 0)
                {
                    // read rest of words with variable encoding
                    for (; ioffs < instrLen; ++ioffs)
                    {
                        if (!ReadVarint(input, out val))
                        {
                            ThrowDecodeException();
                        }
                        words.Add(val);
                    }
                }
                else
                {
                    // read rest of words without any encoding
                    for (; ioffs < instrLen; ++ioffs)
                    {
                        if (input.BaseStream.Position + 4 > input.BaseStream.Length)
                        {
                            ThrowDecodeException();
                        }
                        words.Add(input.ReadUInt32());
                    }
                }
                yield return new SpvOperandCode((SpvOperand)(int)op, [..words]);
            }
        }

        private static bool ReadVarint(BinaryReader input, out uint value)
        {
            uint v = 0;
            int shift = 0;
            while (input.BaseStream.Position < input.BaseStream.Length)
            {
                byte b = input.ReadByte();
                v |= unchecked((uint)(b & 0x7F) << shift);
                shift += 7;
                if ((b & 0x80) == 0)
                {
                    break;
                }
            }

            value = v;
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
