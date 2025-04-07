using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.LuaDecompiler
{
    public partial class LuaJitReader
    {
        const int BCDUMP_KGC_CHILD = 0;
        const int BCDUMP_KGC_TAB = 1;
        const int BCDUMP_KGC_I64 = 2;
        const int BCDUMP_KGC_U64 = 3;
        const int BCDUMP_KGC_COMPLEX = 4;
        const int BCDUMP_KGC_STR = 5;

        const int BCDUMP_KTAB_NIL = 0;
        const int BCDUMP_KTAB_FALSE = 1;
        const int BCDUMP_KTAB_TRUE = 2;
        const int BCDUMP_KTAB_INT = 3;
        const int BCDUMP_KTAB_NUM = 4;
        const int BCDUMP_KTAB_STR = 5;

        public const string Signature = "\x1BLJ";

        public LuaBytecode ReadBytecode(Stream stream)
        {
            var data = new LuaBytecode();
            ReadHeader(stream, data.Header);
            var reader = new BundleBinaryReader(stream, data.Header.Endianness);
            var name = string.Empty;
            if (!data.Header.Flags!.IsStripped)
            {
                name = Encoding.UTF8.GetString(
                    reader.ReadBytes(reader.Read7BitEncodedInt()));
            }
            var extractor = new JitOperandExtractor(data.Header.Version);
            var items = new Queue<LuaChunk>();
            while (true)
            {
                var chunk = ReadChunk(reader, data.Header, items, extractor);
                if (chunk is null)
                {
                    break;
                }
                items.Enqueue(chunk);
            }
            if (!items.TryDequeue(out var item))
            {
                return data;
            }
            data.MainChunk = item;
            data.MainChunk.Name = name;
            return data;
        }

        private LuaChunk? ReadChunk(
            BundleBinaryReader reader, 
            LuaHeader header,
            Queue<LuaChunk> protoItems,
            JitOperandExtractor extractor)
        {
            var size = reader.Read7BitEncodedInt();
            var nextPosition = reader.Position + size;
            if (size == 0)
            {
                return null;
            }
            var chunk = new LuaChunk();
            chunk.Flags = new LuaProtoFlags(reader.ReadByte());
            chunk.ParameterCount = reader.ReadByte();
            chunk.MaxStack = reader.ReadByte(); // framesize
            chunk.UpValueCount = reader.ReadByte();
            var constantsCount = reader.Read7BitEncodedInt();
            var numericConstantsCount = reader.Read7BitEncodedInt();
            var instructionsCount = reader.Read7BitEncodedInt();
            var debugInfoSize = 0L;
            if (!header.Flags!.IsStripped)
            {
                debugInfoSize = reader.Read7BitEncodedInt64();
            }
            var lineCount = 0UL;
            if (debugInfoSize > 0)
            {
                chunk.LineDefined = (ulong)reader.Read7BitEncodedInt64();
                lineCount = (ulong)reader.Read7BitEncodedInt64();
            }
            chunk.LastLineDefined = chunk.LineDefined + lineCount;
            chunk.OpcodeItems = reader.ReadArray(instructionsCount, (_, _) => {
                return extractor.Extract(reader.ReadBytes(4));
            });
            chunk.DebugInfo.UpValueItems = reader.ReadArray(chunk.UpValueCount, (_, _) => {
                var code = reader.ReadUInt16();
                return new LuaUpValue()
                {
                    inStack = (code & 0x8000) != 0,
                    Idx = (byte)(code & 0x7FFF),
                };
            });
            var prototypeItems = new List<LuaChunk>();
            chunk.ConstantItems = reader.ReadArray(constantsCount, (_, _) => {
                var type = reader.Read7BitEncodedInt();
                switch (type)
                {
                    case BCDUMP_KGC_COMPLEX:
                        return new LuaConstant(LuaConstantType.Complex,
                            (CombineUInt((uint)reader.Read7BitEncodedInt(), (uint)reader.Read7BitEncodedInt(), header),
                            CombineUInt((uint)reader.Read7BitEncodedInt(), (uint)reader.Read7BitEncodedInt(), header))
                        );
                    case BCDUMP_KGC_U64:
                        return new LuaConstant(LuaConstantType.Number, 
                            CombineUInt((uint)reader.Read7BitEncodedInt(), (uint)reader.Read7BitEncodedInt(), header));
                    case BCDUMP_KGC_I64:
                        return new LuaConstant(LuaConstantType.Number,
                            CombineInt((uint)reader.Read7BitEncodedInt(), (uint)reader.Read7BitEncodedInt64(), header));
                    case BCDUMP_KGC_TAB:
                        return ReadTable(reader, header);
                    case BCDUMP_KGC_CHILD:
                        if (protoItems.TryDequeue(out var proto))
                        {
                            prototypeItems.Add(proto);
                            return new LuaConstant(LuaConstantType.Proto, prototypeItems.Count - 1);
                        }
                        return new LuaConstant(LuaConstantType.Proto, -1);
                    case >= BCDUMP_KGC_STR:
                        return new LuaConstant(Encoding.UTF8.GetString(reader.ReadBytes(type - BCDUMP_KGC_STR)));
                    default:
                        throw new ArgumentException($"BCDUMP_KGC: {type}");
                }
            }).Reverse().ToArray();
            chunk.NumberConstantItems = reader.ReadArray(numericConstantsCount, (_, _) => {
                var isNum = (reader.ReadByte() & 0x1) != 0;
                var lo = (uint)reader.Read7BitEncodedInt();
                if (!isNum)
                {
                    return new LuaConstant(LuaConstantType.Number, lo);
                }
                return new LuaConstant(LuaConstantType.Number, CombineFloat(lo, (uint)reader.Read7BitEncodedInt(), header));
            });
            chunk.PrototypeItems = [..prototypeItems];
            if (debugInfoSize > 0)
            {
                // reader.BaseStream.Seek(debugInfoSize, SeekOrigin.Current);
                chunk.DebugInfo.AbsoluteLineItems = reader.ReadArray(instructionsCount, (_, _) => {
                    return new LuaLineInfo(chunk.LineDefined + lineCount switch
                    {
                        >= 65536 => reader.ReadUInt32(),
                        >= 256 => reader.ReadUInt16(),
                        _ => reader.ReadByte(),
                    }, 0u);
                });
                chunk.DebugInfo.UpValueNameItems = reader.ReadArray(chunk.UpValueCount, (_, _) => {
                    return reader.ReadStringZeroTerm();
                });
                var lastPc = 0UL;
                var localItems = new List<LuaLocalVar>();
                while (true)
                {
                    var internalVarType = reader.ReadByte();
                    if (internalVarType == 0)
                    {
                        break;
                    }
                    var item = new LuaLocalVar();
                    if (internalVarType >= 7)
                    {
                        reader.Position--;
                        item.Name = reader.ReadStringZeroTerm();
                    }
                    item.StartPc = lastPc + (ulong)reader.Read7BitEncodedInt64();
                    item.EndPc = item.StartPc + (ulong)reader.Read7BitEncodedInt64();
                    lastPc = item.StartPc;
                    localItems.Add(item);
                }
                chunk.DebugInfo.LocalItems = [.. localItems];
            }
            reader.BaseStream.Seek(nextPosition, SeekOrigin.Begin);
            return chunk;
        }

        private LuaConstant ReadTable(BundleBinaryReader reader, LuaHeader header)
        {
            var nArray = reader.Read7BitEncodedInt();
            var nHash = reader.Read7BitEncodedInt();
            var items = reader.ReadArray(nArray, (_, _) => {
                return ReadTableValue(reader, header);
            });
            var hashItems = reader.ReadArray(nArray, (_, _) => {
                return (ReadTableValue(reader, header), ReadTableValue(reader, header));
            });
            return new LuaConstant(LuaConstantType.Table, new LuaConstantTable()
            {
                Items = items,
                HashItems = hashItems
            });
        }

        private LuaConstant ReadTableValue(BundleBinaryReader reader, LuaHeader header)
        {
            var type = reader.Read7BitEncodedInt();
            return type switch
            {
                BCDUMP_KTAB_NIL => new LuaConstant(),
                BCDUMP_KTAB_FALSE => new LuaConstant(false),
                BCDUMP_KTAB_TRUE => new LuaConstant(true),
                BCDUMP_KTAB_INT => new LuaConstant(LuaConstantType.Number, reader.Read7BitEncodedInt()),
                BCDUMP_KTAB_NUM => new LuaConstant(LuaConstantType.Number, CombineFloat((uint)reader.Read7BitEncodedInt(), (uint)reader.Read7BitEncodedInt(), header)),
                >= BCDUMP_KTAB_STR => new LuaConstant(Encoding.UTF8.GetString(reader.ReadBytes(type - BCDUMP_KTAB_STR))),
                _ => throw new ArgumentException($"BCDUMP_KTAB: {type}")
            };
        }

        private long CombineInt(uint lo, uint hi, LuaHeader header)
        {
            if (header.Endianness == EndianType.BigEndian)
            {
                return ((long)lo << 32) | hi;
            }
            return ((long)hi << 32) | lo;
        }
        private ulong CombineUInt(uint lo, uint hi, LuaHeader header)
        {
            if (header.Endianness == EndianType.BigEndian)
            {
                return ((ulong)lo << 32) | hi;
            }
            return ((ulong)hi << 32) | lo;
        }

        private double CombineFloat(uint lo, uint hi, LuaHeader header)
        {
            return BitConverter.UInt64BitsToDouble(CombineUInt(lo, hi, header));
        }

        private void ReadHeader(Stream stream, LuaHeader header)
        {
            stream.Seek(Signature.Length, SeekOrigin.Current);
            header.Version = (LuaVersion)stream.ReadByte();
            var flags = new LuaHeaderFlags((byte)stream.ReadByte());
            header.Endianness = flags.IsBigEndian ? EndianType.BigEndian : EndianType.LittleEndian;
            header.SizeOfInt = 4;
            header.SizeOfSizeT = 4;
            header.SizeOfInstruction = 4;
            header.SizeOfNumber = 4;
            header.Flags = flags;
        }
    }
}
