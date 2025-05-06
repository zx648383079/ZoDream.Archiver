using System.Collections.Generic;
using System.IO;
using ZoDream.Shared.Converters;
using ZoDream.Shared.Language;

namespace ZoDream.SourceGenerator
{
    public class SourceWriter(ILexer input) : ILanguageWriter
    {
        private readonly ICodeWriter _cvtWriter = new CodeWriter();
        private readonly HashSet<string> _cvtItems = [];
        private readonly HashSet<string> _propertyItems = [];

        public void Write(Stream output)
        {
            var writer = new CodeWriter(output);
            Write(writer);
            _cvtWriter.Dispose();
        }

        public void Write(ICodeWriter writer)
        {
            var isAppend = true;
            while (input.MoveNext())
            {
                var token = input.Current;
                if (token.Type == TokenType.Eof)
                {
                    break;
                }
                if (token.Type != TokenType.ReservedKeywords)
                {
                    continue;
                }
                if (token.Value == "namespace")
                {
                    isAppend = false;
                    WriteNamespace(writer);
                }
                else if (token.Value == "struct")
                {
                    WriteStruct(writer);
                }
                else if (token.Value == "enum")
                {
                    WriteEnum(writer);
                }
            }
            if (isAppend)
            {
                WriteRegister(writer);
            }
        }

        private void WriteEnum(ICodeWriter writer)
        {
            writer.WriteLine(true)
                .Write("internal enum ");
            while (input.MoveNext())
            {
                var token = input.Current;
                if (token.Type == TokenType.BraceOpen)
                {
                    break;
                }
                if (token.Type == TokenType.NewLine)
                {
                    continue;
                }
                writer.Write(" ");
                if (token.Type == TokenType.DataType)
                {
                    writer.Write(TranslateType(token.Value));
                    continue;
                }
                writer.Write(token.Value);
            }
            writer.WriteLine(true)
                .Write("{").WriteIndentLine();
            while (input.MoveNext())
            {
                var token = input.Current;
                if (token.Type == TokenType.BraceClose)
                {
                    break;
                }
                if (token.Type == TokenType.NewLine)
                {
                    continue;
                }
                if (token.Type == TokenType.Comma)
                {
                    writer.Write(',').WriteLine(true);
                    continue;
                }
                writer.Write(" ");
                if (token.Type == TokenType.DataType)
                {
                    writer.Write(TranslateType(token.Value));
                    continue;
                }
                writer.Write(token.Value);
            }
            writer.WriteOutdentLine().Write("}").WriteLine(true);
        }

        private void WriteStruct(ICodeWriter writer)
        {
            _propertyItems.Clear();
            writer.WriteLine(true)
                .Write("internal class ");
            _cvtWriter.WriteLine(true)
                .Write("internal class ");
            var cls = string.Empty;
            while (input.MoveNext())
            {
                var token = input.Current;
                if (token.Type == TokenType.BraceOpen)
                {
                    break;
                }
                if (token.Type == TokenType.NewLine)
                {
                    continue;
                }
                cls = token.Value;
            }

            writer.Write(cls).WriteLine(true)
                .Write("{").WriteIndentLine();
            _cvtItems.Add(cls);
            _cvtWriter.WriteFormat("{0}Converter : BundleConverter<{0}>", cls)
                .WriteLine(true)
                .Write("{")
                .WriteIndentLine()
                .WriteFormat("public override {0} Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)", cls)
                .WriteLine(true)
                .Write("{")
                .WriteIndentLine()
                .WriteFormat("var res = new {0}();", cls).WriteLine(true);
            WriteBlock(writer);
            writer.WriteOutdentLine().Write("}").WriteLine(true);
            _cvtWriter.Write("return res;")
                .WriteOutdentLine().Write("}")
                .WriteOutdentLine().Write("}").WriteLine(true);
        }

        private void WriteBlock(ICodeWriter writer)
        {
            while (input.MoveNext())
            {
                var token = input.Current;
                if (token.Type == TokenType.BraceClose)
                {
                    break;
                }
                if (token.Type is TokenType.NewLine or TokenType.Semicolon or 
                    TokenType.Comment)
                {
                    continue;
                }
                WriteProperty(writer);
                if (input.Current.Type == TokenType.BraceClose)
                {
                    break;
                }
            }
        }

        private void WriteProperty(ICodeWriter writer)
        {
            do
            {
                var token = input.Current;
                if (token.Type is TokenType.NewLine or TokenType.Semicolon)
                {
                    continue;
                }
                if (token.Type == TokenType.ReservedKeywords)
                {
                    if  (token.Value == "padding")
                    {
                        input.MoveNext();
                        input.MoveNext();
                        _cvtWriter.WriteFormat("reader.Position += {0};", input.Current.Value)
                            .WriteLine(true);
                        JumpToEnd();
                        break;
                    } else if (token.Value is "if" or "else")
                    {
                        WriteSource(TokenType.BraceOpen);
                        _cvtWriter.Write(" {").WriteIndentLine();
                        WriteBlock(writer);
                        _cvtWriter.WriteOutdentLine().Write("}").WriteLine(true);
                        continue;
                    }
                }
                var (type, childType) = GetPropertyType();
                if (type == "assert")
                {
                    JumpToEnd();
                    break;
                }
                if (type == "AlignTo")
                {
                    _cvtWriter.WriteFormat("reader.AlignStream({0});", childType)
                        .WriteLine(true);
                    JumpToEnd();
                    break;
                }
                var name = string.Empty;
                var isArray = false;
                if (input.Current.Type != TokenType.Semicolon)
                {
                    name = StringConverter.Studly(input.Current.Value);
                    input.MoveNext();
                    if (input.Current.Type == TokenType.BracketOpen)
                    {
                        isArray = true;
                    } else if (input.Current.Type == TokenType.Equal)
                    {
                        if (_propertyItems.Contains(name))
                        {
                            _cvtWriter.WriteFormat("res.{0} ", name);
                        } else
                        {
                            _cvtWriter.WriteFormat("var {0} ", name);
                        }
                        WriteSource(TokenType.Semicolon);
                        _cvtWriter.Write(';').WriteLine(true);
                        JumpToEnd();
                        break;
                    }
                }
                var propertyType = type;
                var ltText = !string.IsNullOrEmpty(name) ? $"res.{name} = " : string.Empty;
                if (type == "List")
                {
                    isArray = true;
                    propertyType = childType;
                    _cvtWriter.WriteFormat("{0}reader.ReadArray(_ => {1});", 
                        ltText, TranslateReadType(childType));
                } else if (isArray)
                {
                    if (type == "char")
                    {
                        _cvtWriter.WriteFormat("{0}reader.ReadString(",
                            ltText);
                    } else
                    {
                        _cvtWriter.WriteFormat("{0}reader.ReadArray(", ltText);
                    }
                    input.MoveNext();
                    WriteSource(TokenType.BracketClose);
                    if (type == "char")
                    {
                        _cvtWriter.Write(");");
                    }
                    else
                    {
                        _cvtWriter.WriteFormat(", _ => {0});",
                            TranslateReadType(type));
                    }
                }
                else
                {
                    _cvtWriter.WriteFormat("{0}{1};", 
                        ltText, TranslateReadType(type));
                }
                _cvtWriter.WriteLine(true);
                if (propertyType is "char" or "char16")
                {
                    isArray = false;
                }
                propertyType = TranslateType(propertyType);
                if (isArray)
                {
                    propertyType += "[]";
                }
                if (!string.IsNullOrEmpty(name) && !_propertyItems.Contains(name))
                {
                    writer.WriteFormat("public {0} {1};", propertyType, name).WriteLine(true);
                    _propertyItems.Add(name);
                }
                JumpToEnd();
                break;
            }
            while (input.MoveNext());
        }

        /// <summary>
        /// 原样输出
        /// </summary>
        private void WriteSource(TokenType end)
        {
            bool nextCanMove;
            do
            {
                nextCanMove = true;
                var token = input.Current;
                if (token.Type == end)
                {
                    break;
                }
                if (token.Type is TokenType.Comment or TokenType.NewLine)
                {
                    continue;
                }
                if (token.Value == "$")
                {
                    _cvtWriter.Write("reader.Position");
                    continue;
                }
                if (token.Type is not TokenType.Identifier)
                {
                    _cvtWriter.Write(token.Value);
                    continue;
                }
                var text = token.Value;
                var isProperty = true;
                while (input.MoveNext())
                {
                    var next = input.Current;
                    if (next.Type == end)
                    {
                        break;
                    }
                    if (next.Type is TokenType.ParenOpen)
                    {
                        isProperty = false;
                        break;
                    }
                    if (IsDoubleColon(next))
                    {
                        isProperty = false;
                    } else if (next.Type is not TokenType.Identifier)
                    {
                        break;
                    }
                    text += next.Value;
                }
                if (isProperty)
                {
                    text = StringConverter.Studly(text);
                    if (_propertyItems.Contains(text))
                    {
                        _cvtWriter.Write("res.");
                    }
                    _cvtWriter.Write(text);
                } else if (text.EndsWith("::eof"))
                {
                    input.MoveNext();
                    input.MoveNext();
                    _cvtWriter.Write("(reader.RemainingLength <= 0)");
                } else if (text.Contains("::mem::"))
                {
                    _cvtWriter.Write("reader.").Write(StringConverter.Studly(text[(text.LastIndexOf(':') + 1)..]));
                } else
                {
                    _cvtWriter.Write(text);
                }
                nextCanMove = false;
            }
            while (!nextCanMove || input.MoveNext());
        }

        private (string, string) GetPropertyType()
        {
            var res = input.Current.Value;
            var childType = string.Empty;
            while (input.MoveNext())
            {
                var token = input.Current;
                if (token.Type == TokenType.GreaterThan)
                {
                    break;
                }
                if (IsDoubleColon(token))
                {
                    input.MoveNext();
                    res = input.Current.Value;
                    continue;
                }
                if (token.Type == TokenType.LessThan)
                {
                    input.MoveNext();
                    (childType, _) = GetPropertyType();
                    input.MoveNext();
                    break;
                }
                break;
            }
            return (res, childType);
        }

        private void JumpToEnd()
        {
            do
            {
                if (input.Current.Type is TokenType.Eof
                                    or TokenType.BraceClose or TokenType.Semicolon)
                {
                    break;
                }
            } while (input.MoveNext());
        }

        private static bool IsDoubleColon(Token token)
        {
            return token.Type == TokenType.CompoundOperator && token.Value == "::";
        }
        private void WriteNamespace(ICodeWriter writer)
        {
            writer.WriteLine(true)
                .Write("namespace ");
            while (input.MoveNext())
            {
                var token = input.Current;
                if (token.Type == TokenType.BraceOpen)
                {
                    break;
                }
                if (token.Type == TokenType.NewLine)
                {
                    continue;
                }
                writer.Write(token.Value);
            }
            writer.WriteLine(true)
                .Write("{").WriteIndentLine();
            _cvtWriter.WriteIndent(writer.Indent, true).WriteLine(true);
            while (input.MoveNext())
            {
                var token = input.Current;
                if (token.Type == TokenType.BraceClose)
                {
                    break;
                }
                if (token.Value == "struct")
                {
                    WriteStruct(writer);
                }
                else if (token.Value == "enum")
                {
                    WriteEnum(writer);
                }
            }
            WriteRegister(writer);
            writer.WriteOutdentLine().Write("}").WriteLine(true);
        }

        /// <summary>
        /// 合并数据并追加
        /// </summary>
        /// <param name="writer"></param>
        private void WriteRegister(ICodeWriter writer)
        {
            writer.Write(_cvtWriter);
            writer.WriteLine(true)
                .Write("internal partial static class Engine").WriteLine(true)
                .Write('{').WriteIndentLine()
                .Write("public static IBundleConverter[] Converters = [").WriteIndentLine();
            foreach (var item in _cvtItems)
            {
                writer.WriteFormat("new {0}Converter(),", item).WriteLine(true);
            }
            writer.WriteOutdentLine()
                .Write("];").WriteOutdentLine()
                .Write('}').WriteLine(true);
        }

        private static string TranslateType(string text)
        {
            return text switch 
            {
                "u8" => "byte",
                "u16" => "ushort",
                "u24" or "u32" => "uint",
                "u48" or "u64" => "ulong",
                "s8" => "sbyte",
                "s16" => "short",
                "s24" or "s32" => "int",
                "s48" or "s64" or "LEB128" => "long",
                "char" or "str" or "char16" or "AlignString" or "Leb128String" => "string",
                _ => text
            };
        }

        private static string TranslateReadType(string text)
        {
            return text switch
            {
                "u8" => "reader.ReadByte()",
                "u16" => "reader.ReadUInt16()",
                "u24" or "u32" => "reader.ReadUInt32()",
                "u48" or "u64" => "reader.ReadUInt64()",
                "s8" => "reader.ReadByte()",
                "s16" => "reader.ReadInt16()",
                "s24" or "s32" => "reader.ReadInt32()",
                "s48" or "s64" => "reader.ReadInt64()",
                "float" => "reader.ReadSingle()",
                "double" => "reader.ReadDouble()",
                "bool" => "reader.ReadBoolean()",
                "AlignString" => "reader.ReadAlignedString()",
                "Leb128String" => "reader.ReadLeb128String()",
                "LEB128" => "reader.Read7BitEncodedInt()",
                "Vector3" or "Vector2" or "Vector4" or "Quaternion" => $"reader.Read{text}()",
                _ => $"serializer.Deserialize<{text}>(reader)"
            };
        }
    }
}
