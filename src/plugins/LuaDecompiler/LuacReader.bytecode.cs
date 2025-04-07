using System;
using System.IO;
using System.Text;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.LuaDecompiler
{
    public partial class LuacReader
    {
        public const string Signature = "\x1BLua";

        public LuaBytecode ReadBytecode(Stream stream)
        {
            var data = new LuaBytecode();
            var reader = ReadHeader(stream, data.Header);
            var extractor = new OperandExtractor(data.Header.Version);
            data.MainChunk = ReadChunk(reader, data.Header, extractor);
            return data;
        }



        private LuaChunk ReadChunk(IBundleBinaryReader reader, 
            LuaHeader header, OperandExtractor extractor)
        {
            var chunk = new LuaChunk();
            if (header.Version != LuaVersion.Lua52)
            {
                chunk.Name = ReadString(reader, header);
            }
            if (header.Version == LuaVersion.Lua54)
            {
                chunk.LineDefined = ReadLeb54(reader);
                chunk.LastLineDefined = ReadLeb54(reader);
            } else
            {
                chunk.LineDefined = ReadInt(reader, header);
                chunk.LastLineDefined = ReadInt(reader, header);
            }
            if (header.Version == LuaVersion.Lua51)
            {
                chunk.UpValueCount = reader.ReadByte();
            }
            chunk.ParameterCount = reader.ReadByte();
            var isVarArg = reader.ReadByte();
            if (header.Version == LuaVersion.Lua51 && (isVarArg & 2) != 0)
            {
                chunk.VarArg = new LuaVarArgInfo(isVarArg);
            } else if (header.Version != LuaVersion.Lua51 && isVarArg != 0)
            {
                chunk.VarArg = new() { NeedArg = true, HasArg = true };
            }
            chunk.MaxStack = reader.ReadByte();
            var opcodeItems = ReadArray(reader, header, () => {
                return header.SizeOfInstruction == 4 ? reader.ReadUInt32() : 0;
            });
            chunk.OpcodeItems = extractor.Extract(opcodeItems);
            chunk.ConstantItems = ReadArray(reader, header, () => {
                return ReadConstant(reader, header);
            });
            if (header.Version >= LuaVersion.Lua53)
            {
                chunk.DebugInfo.UpValueItems = ReadArray(reader, header, () => {
                    var val = new LuaUpValue()
                    {
                        inStack = reader.ReadBoolean(),
                        Idx = reader.ReadByte(),
                    };
                    if (header.Version == LuaVersion.Lua54)
                    {
                        val.Kind = reader.ReadByte();
                    }
                    return val;
                });
            }
            chunk.PrototypeItems = ReadArray(reader, header, () => {
                return ReadChunk(reader, header, extractor);
            });
            if (header.Version == LuaVersion.Lua52)
            {
                chunk.DebugInfo.UpValueItems = ReadArray(reader, header, () => {
                    return new LuaUpValue()
                    {
                        inStack = reader.ReadBoolean(),
                        Idx = reader.ReadByte(),
                    };
                });
            }
            if (header.Version == LuaVersion.Lua54)
            {
                chunk.DebugInfo.LineNoItems = ReadArray(reader, header, () => {
                    return (uint)reader.ReadByte();
                });
            }
            chunk.DebugInfo.AbsoluteLineItems = ReadArray(reader, header, () => {
                if (header.Version == LuaVersion.Lua54)
                {
                    return new LuaLineInfo((uint)ReadLeb54(reader), (uint)ReadLeb54(reader));
                }
                return new LuaLineInfo((uint)ReadInt(reader, header), 0u);
            });
            chunk.DebugInfo.LocalItems = ReadArray(reader, header, () => {
                return ReadLocal(reader, header);
            });
            chunk.DebugInfo.UpValueNameItems = ReadArray(reader, header, () => {
                return ReadString(reader, header);
            });
            return chunk;
        }

        private LuaConstant ReadConstant(IBundleBinaryReader reader, LuaHeader header)
        {
            var code = reader.ReadByte();
            if (header.Version == LuaVersion.Lua54)
            {
                return code switch
                {
                    0 => new LuaConstant(),
                    1 => new LuaConstant(false),
                    0x11 => new LuaConstant(true),
                    0x3 => new LuaConstant(LuaConstantType.Number, reader.ReadUInt64()),
                    0x13 => new LuaConstant(LuaConstantType.Number, reader.ReadDouble()),
                    4 or 0x14 => new LuaConstant(ReadString(reader, header)),
                    _ => throw new ArgumentException("count constants error"),
                };
            }
            if (header.Version == LuaVersion.Lua53)
            {
                if (code == 3)
                {
                    return new LuaConstant(LuaConstantType.Number, reader.ReadDouble());
                }
                else if (code is 4 or 0x14)
                {
                    return new LuaConstant(ReadString(reader, reader.ReadByte(), header));
                }
                else if (code == 0x13)
                {
                    return new LuaConstant(LuaConstantType.Number, reader.ReadUInt64());
                }
            }
            return code switch
            {
                0 => new LuaConstant(),
                1 => new LuaConstant(reader.ReadBoolean()),
                3 => new LuaConstant(LuaConstantType.Number, ReadNumber(reader, header)),
                4 => new LuaConstant(ReadString(reader, header)),
                _ => throw new ArgumentException("count constants error"),
            };
        }

        private LuaLocalVar ReadLocal(IBundleBinaryReader reader, LuaHeader header)
        {
            var val = new LuaLocalVar()
            {
                Name = ReadString(reader, header),
            };
            if (header.Version == LuaVersion.Lua54)
            {
                val.StartPc = ReadLeb54(reader);
                val.EndPc = ReadLeb54(reader);
            } else
            {
                val.StartPc = ReadInt(reader, header);
                val.EndPc = ReadInt(reader, header);
            }
            return val;
        }


        public T[] ReadArray<T>(IBundleBinaryReader reader, LuaHeader header, Func<T> cb)
        {
            var count = header.Version >= LuaVersion.Lua54 ? 
                (int)ReadLeb54(reader) : (int)ReadInt(reader, header);
            return reader.ReadArray(count, (_, _) => {
                return cb.Invoke();
            });
        }

        private string ReadString(IBundleBinaryReader reader, LuaHeader header)
        {
            var count = 0;
            if (header.Version is LuaVersion.Lua51 or LuaVersion.Lua52)
            {
                count = (int)ReadSizeT(reader, header);
            } else if (header.Version == LuaVersion.Lua53)
            {
                count = reader.ReadByte();
                if (count == byte.MaxValue)
                {
                    count = (int)reader.ReadUInt64();
                }
            } else if (header.Version == LuaVersion.Lua54)
            {
                count = (int)ReadLeb54(reader);
            }
            return ReadString(reader, count, header);
        }

        private string ReadString(IBundleBinaryReader reader, 
            int length, LuaHeader header)
        {
            if (length < 1)
            {
                return string.Empty;
            }
            var buffer = reader.ReadBytes(length - 1);
            if (header.Version < LuaVersion.Lua53)
            {
                reader.ReadByte(); // 去除结尾的 0x0
            }
            return Encoding.UTF8.GetString(buffer);
        }

        private object ReadNumber(IBundleBinaryReader reader, LuaHeader header)
        {
            if (header.IsNumberIntegral)
            {
                return ReadInt(reader, header.SizeOfNumber);
            }
            return header.SizeOfNumber switch
            {
                8 => reader.ReadDouble(),
                4 => reader.ReadSingle(),
                _ => 0f,
            };
        }

        private ulong ReadInt(IBundleBinaryReader reader, LuaHeader header)
        {
            return ReadInt(reader, header.SizeOfInt);
        }

        /// <summary>
        /// lua54 大端 leb128 
        /// </summary>
        /// <param name="reader"></param>
        /// <returns></returns>
        private ulong ReadLeb54(IBundleBinaryReader reader)
        {
            var maxLength = 9;
            ulong res = 0;
            byte b;
            for (var i = 0; i < maxLength; i ++)
            {
                res <<= 7;
                b = reader.ReadByte();
                res |= (uint)(b & 0x7f);
                if ((b & 0x80) != 0)
                {
                    return res;
                }
            }
            return res;
        }

        private ulong ReadSizeT(IBundleBinaryReader reader, LuaHeader header)
        {
            return ReadInt(reader, header.SizeOfSizeT);
        }
        private ulong ReadInt(IBundleBinaryReader reader, byte size)
        {
            return size switch
            {
                8 => reader.ReadUInt64(),
                4 => reader.ReadUInt32(),
                2 => reader.ReadUInt16(),
                1 => reader.ReadByte(),
                _ => 0,
            };
        }

        private IBundleBinaryReader ReadHeader(Stream stream, LuaHeader header)
        {
            stream.Seek(Signature.Length, SeekOrigin.Current);
            header.Endianness = EndianType.LittleEndian;
            header.Version = (LuaVersion)stream.ReadByte();
            header.FormatVersion = (byte)stream.ReadByte();
            if (header.Version is LuaVersion.Lua51 or LuaVersion.Lua52)
            {
                header.Endianness = stream.ReadByte() == 1 ? EndianType.LittleEndian : EndianType.BigEndian;
            }
            var reader = new BundleBinaryReader(stream, header.Endianness);
            if (header.Version is LuaVersion.Lua51 or LuaVersion.Lua52)
            {
                header.SizeOfInt = (byte)stream.ReadByte();
                header.SizeOfSizeT = (byte)stream.ReadByte();
                header.SizeOfInstruction = (byte)stream.ReadByte();
                header.SizeOfNumber = (byte)stream.ReadByte();
                header.IsNumberIntegral = stream.ReadByte() > 0;
                if (header.Version == LuaVersion.Lua52)
                {
                    stream.Seek(6, SeekOrigin.Current); //  LUAC_DATA
                }
            } else if (header.Version == LuaVersion.Lua53)
            {
                stream.Seek(6, SeekOrigin.Current); //  LUAC_DATA
                header.SizeOfInt = (byte)stream.ReadByte();
                header.SizeOfSizeT = (byte)stream.ReadByte();
                header.SizeOfInstruction = (byte)stream.ReadByte();
                stream.Seek(1, SeekOrigin.Current); // lua_Integer
            }
            else if (header.Version == LuaVersion.Lua54)
            {
                stream.Seek(6, SeekOrigin.Current); //  LUAC_DATA
                header.SizeOfInt = reader.ReadByte();
                header.SizeOfSizeT = reader.ReadByte(); // lua_Integer
            }
            if (header.Version is LuaVersion.Lua54 or LuaVersion.Lua53)
            {
                header.SizeOfInstruction = 4;
                header.SizeOfNumber = reader.ReadByte();
                header.LuacInt = reader.ReadUInt64();
                header.LuacNumber = reader.ReadDouble();
                header.SizeOfUpValue = reader.ReadByte();
            }
            return reader;
        }
    }
}
