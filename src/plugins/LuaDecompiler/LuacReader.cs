using System;
using System.IO;
using System.Text;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.LuaDecompiler
{
    public class LuacReader: ILanguageReader
    {
        internal const string Signature = "\x1BLua";

        public LuaBytecode Read(Stream stream)
        {
            var data = new LuaBytecode();
            ReadHeader(stream, data.Header);
            data.MainChunk = ReadChunk(new BundleBinaryReader(stream, data.Header.Endianness), data.Header);
            return data;
        }



        private LuaChunk ReadChunk(IBundleBinaryReader reader, LuaHeader header)
        {
            var chunk = new LuaChunk();
            if (header.Version != LuaVersion.Lua52)
            {
                chunk.Name = ReadString(reader, header);
            }
            chunk.LineDefined = ReadInt(reader, header);
            chunk.LastLineDefined = ReadInt(reader, header);
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
            chunk.InstructionItems = reader.ReadArray((int)ReadInt(reader, header), (_, _) => {
                if (header.SizeOfInstruction == 4)
                {
                    return reader.ReadUInt32();
                }
                return 0u;
            });
            chunk.ConstantItems = reader.ReadArray((int)ReadInt(reader, header), (_, _) => {
                return ReadConstant(reader, header);
            });
            if (header.Version == LuaVersion.Lua53)
            {
                chunk.UpValueItems = reader.ReadArray((int)ReadInt(reader, header), (_, _) => {
                    return new LuaUpValue()
                    {
                        inStack = reader.ReadBoolean(),
                        Idx = reader.ReadByte(),
                    };
                });
            } else if (header.Version == LuaVersion.Lua54)
            {
                chunk.UpValueItems = reader.ReadArray((int)ReadInt(reader, header), (_, _) => {
                    return new LuaUpValue()
                    {
                        inStack = reader.ReadBoolean(),
                        Idx = reader.ReadByte(),
                        Kind = reader.ReadByte(),
                    };
                });
            }
            chunk.PrototypeItems = reader.ReadArray((int)ReadInt(reader, header), (_, _) => {
                return ReadChunk(reader, header);
            });
            if (header.Version == LuaVersion.Lua52)
            {
                chunk.UpValueItems = reader.ReadArray((int)ReadInt(reader, header), (_, _) => {
                    return new LuaUpValue()
                    {
                        inStack = reader.ReadBoolean(),
                        Idx = reader.ReadByte(),
                    };
                });
            }
            if (header.Version == LuaVersion.Lua54)
            {
                var lineInfo = reader.ReadArray((int)ReadInt(reader, header), (_, _) => {
                    return reader.ReadByte();
                });
            }
            chunk.SourceLineItems = reader.ReadArray((int)ReadInt(reader, header), (_, _) => {
                var pc = (uint)ReadInt(reader, header);
                var line = header.Version == LuaVersion.Lua54 ? (uint)ReadInt(reader, header) : 0u;
                return (pc, line);
            });
            chunk.LocalItems = reader.ReadArray((int)ReadInt(reader, header), (_, _) => {
                return ReadLocal(reader, header);
            });
            chunk.UpValueNameItems = reader.ReadArray((int)ReadInt(reader, header), (_, _) => {
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
                    return new LuaConstant(ReadString(reader, reader.ReadByte()));
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
            return new LuaLocalVar()
            {
                Name = ReadString(reader, header),
                StartPc = ReadInt(reader, header),
                EndPc = ReadInt(reader, header),
            };
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
                count = reader.ReadByte();
            }
            return ReadString(reader, count);
        }

        private string ReadString(IBundleBinaryReader reader, int length)
        {
            if (length < 1)
            {
                return string.Empty;
            }
            var buffer = reader.ReadBytes(length - 1);
            reader.ReadByte(); // 去除结尾的 0x0
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

        private void ReadHeader(Stream stream, LuaHeader header)
        {
            stream.Seek(Signature.Length, SeekOrigin.Current);
            header.Version = (LuaVersion)stream.ReadByte();
            header.FormatVersion = (byte)stream.ReadByte();
            if (header.Version is LuaVersion.Lua51 or LuaVersion.Lua52)
            {
                header.Endianness = stream.ReadByte() == 1 ? EndianType.LittleEndian : EndianType.BigEndian;
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
                header.SizeOfNumber = (byte)stream.ReadByte();
                stream.Seek(17, SeekOrigin.Current);
            }
            else if (header.Version == LuaVersion.Lua54)
            {
                stream.Seek(6, SeekOrigin.Current); //  LUAC_DATA
                header.SizeOfInstruction = (byte)stream.ReadByte();
                stream.Seek(1, SeekOrigin.Current); // lua_Integer
                header.SizeOfNumber = (byte)stream.ReadByte();
                stream.Seek(17, SeekOrigin.Current);
            }
        }
    }
}
