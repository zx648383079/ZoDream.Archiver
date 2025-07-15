using System;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared;
using ZoDream.Shared.Language;

namespace ZoDream.LuaDecompiler
{
    public partial class LuaWriter
    {
        private void Translate(ICodeWriter builder, LuaChunk chunk, JitOperandCode code)
        {
            var attr = JitOperandExtractor.GetAttribute(code.Operand);
            if (attr is null)
            {
                return;
            }
            var cd = attr.ArgsCount == 3 ? code.C : code.D;
            switch (code.Operand)
            {
                case JitOperand.ISLT:
                case JitOperand.ISGE:
                case JitOperand.ISLE:
                case JitOperand.ISGT:
                case JitOperand.ISEQV:
                case JitOperand.ISNEV:
                case JitOperand.ISEQS:
                case JitOperand.ISNES:
                case JitOperand.ISEQN:
                case JitOperand.ISNEN:
                case JitOperand.ISEQP:
                case JitOperand.ISNEP:
                    builder.WriteFormat("if {0} {1} {2}",
                        TranslateNotSet(chunk, code.A, attr.AType),
                        TranslateOperation(code.Operand),
                        TranslateNotSet(chunk, (int)cd, attr.CDType)
                        );
                    break;
                case JitOperand.ISTC:
                    {
                        var val = TranslateNotSet(chunk, (int)cd, attr.CDType);
                        builder.WriteFormat("{0} = {1}; if {1}",
                            TranslateIsSet(chunk, code.A, attr.AType),
                            val
                            );
                        break;
                    }
                case JitOperand.ISFC:
                    {
                        var val = TranslateNotSet(chunk, (int)cd, attr.CDType);
                        builder.WriteFormat("{0} = {1}; if not {1}",
                            TranslateIsSet(chunk, code.A, attr.AType),
                            val
                            );
                        break;
                    }
                case JitOperand.IST:
                    builder.WriteFormat("if {0}", TranslateNotSet(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.ISF:
                    builder.WriteFormat("if not {0}", TranslateNotSet(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.ISTYPE:
                    break;
                case JitOperand.ISNUM:
                    break;
                case JitOperand.GGET:
                    if (attr.CDType == JitOperandFormat.STR)
                    {
                        _envItems.Add(TranslateValue(chunk, code.A, attr.AType), chunk.ConstantItems[cd].Value.ToString());
                        break;
                    }
                    builder.WriteFormat("{0} = _env[{1}]",
                        TranslateIsSet(chunk, code.A, attr.AType),
                        TranslateValue(chunk, (int)cd, attr.CDType)
                    );
                    break;
                case JitOperand.GSET:
                    builder.WriteFormat("_env[{0}] = {1}",
                        TranslateValue(chunk, (int)cd, attr.CDType),
                        TranslateNotSet(chunk, code.A, attr.AType));
                    break;
                case JitOperand.MOV:
                    {
                        var key = TranslateValue(chunk, (int)cd, attr.CDType);
                        if (_envItems.ContainsKey(TranslateValue(chunk, (int)cd, attr.CDType)))
                        {
                            _envItems.Rename(key, TranslateValue(chunk, code.A, attr.AType));
                            break;
                        }
                        var val = TranslateNotSet(chunk, (int)cd, attr.CDType);
                        _envItems.Add(TranslateIsSet(chunk, code.A, attr.AType),
                            val);
                        // TODO 
                        //builder.WriteFormat("{0} = {1}",
                        //    TranslateIsSet(chunk, code.A, attr.AType),
                        //    TranslateNotSet(chunk, (int)cd, attr.CDType));
                    }
                    break;
                case JitOperand.NOT:
                case JitOperand.UNM:
                case JitOperand.LEN:
                    {
                        var val = TranslateNotSet(chunk, (int)cd, attr.CDType);
                        builder.WriteFormat("{0} = {1}{2}",
                            TranslateIsSet(chunk, code.A, attr.AType),
                            TranslateOperation(code.Operand),
                            val
                        );
                    }
                    break;
                case JitOperand.ADDVN:
                case JitOperand.SUBVN:
                case JitOperand.MULVN:
                case JitOperand.DIVVN:
                case JitOperand.MODVN:
                    {
                        var b = TranslateNotSet(chunk, (int)cd, attr.BType);
                        var c = TranslateNotSet(chunk, code.B, attr.CDType);
                        builder.WriteFormat("{0} = {1} {2} {3}",
                            TranslateIsSet(chunk, code.A, attr.AType),
                            b,
                            TranslateOperation(code.Operand),
                            c);
                    }
                    break;
                case JitOperand.ADDVV:
                case JitOperand.SUBVV:
                case JitOperand.MULVV:
                case JitOperand.DIVVV:
                case JitOperand.MODVV:
                    {
                        var b = TranslateNotSet(chunk, code.B, attr.BType);
                        var c = TranslateNotSet(chunk, (int)cd, attr.CDType);
                        builder.WriteFormat("{0} = {1} {2} {3}",
                            TranslateIsSet(chunk, code.A, attr.AType),
                            b,
                            TranslateOperation(code.Operand),
                            c);
                    }
                    break;
                case JitOperand.ADDNV:
                case JitOperand.SUBNV:
                case JitOperand.MULNV:
                case JitOperand.DIVNV:
                case JitOperand.MODNV:
                    {
                        var b = TranslateNotSet(chunk, (int)cd, attr.CDType);
                        var c = TranslateNotSet(chunk, code.B, attr.BType);
                        builder.WriteFormat("{0} = {1} {2} {3}",
                            TranslateIsSet(chunk, code.A, attr.AType),
                            b,
                            TranslateOperation(code.Operand),
                            c);
                    }
                    break;
                case JitOperand.POW:
                    {
                        var b = TranslateNotSet(chunk, code.B, attr.BType);
                        var c = TranslateNotSet(chunk, (int)cd, attr.CDType);
                        builder.WriteFormat("{0} = {1} ^ {2} (pow)",
                            TranslateIsSet(chunk, code.A, attr.AType),
                            b,
                            c);
                    }
                    break;
                case JitOperand.CAT:
                    {
                        var begin = code.B;
                        var count = (int)cd - code.B + 1;
                        var args = new string[count];
                        for (int i = 0; i < count; i++)
                        {
                            args[i] = TranslateNotSet(chunk, begin + i, JitOperandFormat.VAR);
                        }
                        builder.WriteFormat("{0} = {1}",
                            TranslateIsSet(chunk, code.A, attr.AType), 
                            string.Join(", ", args));
                    }
                    break;
                
                case JitOperand.KNIL:
                    {
                        var begin = code.A;
                        var count = cd - begin + 1;
                        for (var i = 0; i < count; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(
                                TranslateIsSet(chunk, begin + i, JitOperandFormat.VAR)
                             );
                        }
                        builder.Write(" = nil");
                        break;
                    }
                case JitOperand.KSTR:
                case JitOperand.KCDATA:
                case JitOperand.KSHORT:
                case JitOperand.KNUM:
                case JitOperand.KPRI:
                case JitOperand.UGET:
                case JitOperand.USETV:
                case JitOperand.USETS:
                case JitOperand.USETN:
                case JitOperand.USETP:
                    {
                        var val = TranslateNotSet(chunk, (int)cd, attr.CDType);
                        builder.WriteFormat("{0} = {1}",
                            TranslateIsSet(chunk, code.A, attr.AType),
                            val);
                    }
                    break;
                case JitOperand.UCLO:
                    builder.Write("nil uvs >= ").Write(TranslateValue(chunk, code.A, attr.AType))
                        .Write("; goto ")
                        .Write(TranslateValue(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.FNEW:
                    {
                        var fn = TranslateValue(chunk, code.A, attr.AType);
                        if (chunk.MoveNext())
                        {
                            var next = (JitOperandCode)chunk.CurrentOpcode;
                            Expectation.ThrowIfNot(next.Operand == JitOperand.GSET);
                            fn = chunk.ConstantItems[next.D].Value.ToString();
                        }
                        var sub = chunk.PrototypeItems[(int)chunk.ConstantItems[cd].Value];
                        var args = new string[sub.ParameterCount];
                        for (var i = 0; i < args.Length; i++)
                        {
                            args[i] = TranslateNotSet(sub, i, JitOperandFormat.VAR);
                        }
                        // TODO 判断输入参数名
                        builder.WriteFormat("function {0}({1})", fn, string.Join(", ", args))
                            .WriteIndentLine().WriteFormat("-- {0} {1} FNEW {2} {3};",
                                    chunk.CurrentIndex,
                                    chunk.DebugInfo.LineNoItems.Length > chunk.CurrentIndex ?
                                    chunk.DebugInfo.LineNoItems[chunk.CurrentIndex] : 0,
                                    code.A, cd);
                        Decompile(builder, sub);
                        builder.WriteOutdent().Write("end");
                    }
                    break;
                case JitOperand.TNEW:
                    builder.WriteFormat("{0}  = new table(array: {1}, dict: {2})", 
                        TranslateIsSet(chunk, code.A, attr.AType),
                        cd & 0b0000011111111111,
                        Math.Pow(2, cd >> 11));
                    break;
                case JitOperand.TDUP:
                    {
                        var val = TranslateNotSet(chunk, (int)cd, attr.CDType);
                        builder.WriteFormat("{0} = copy {1}", 
                            TranslateIsSet(chunk, code.A, attr.AType),
                            val);
                    }
                    break;
                
                case JitOperand.TGETV:
                    {
                        var b = TranslateNotSet(chunk, code.B, attr.BType);
                        var c = TranslateNotSet(chunk, (int)cd, attr.CDType);
                        builder.WriteFormat("{0} = {1}[{2}]",
                            TranslateIsSet(chunk, code.A, attr.AType),
                            b, 
                            c
                            );
                    }
                    break;
                case JitOperand.TGETS:
                    {
                        var b = TranslateNotSet(chunk, code.B, attr.BType);
                        builder.WriteFormat("{0} = {1}.{2}",
                            TranslateIsSet(chunk, code.A, attr.AType),
                            b,
                            TranslateValue(chunk, (int)cd, attr.CDType));
                    }
                    break;
                case JitOperand.TGETB:
                    {
                        var b = TranslateNotSet(chunk, (int)cd, attr.BType);
                        builder.WriteFormat("{0} = {1}[{2}]",
                              TranslateIsSet(chunk, code.A, attr.AType),
                              b,
                              TranslateValue(chunk, code.B, attr.CDType));
                    }
                    break;
                case JitOperand.TGETR:
                    {
                        var b = TranslateNotSet(chunk, code.B, attr.BType);
                        var c = TranslateNotSet(chunk, (int)cd, attr.CDType);
                        builder.WriteFormat("{0} = {1}[{2}]",
                            TranslateIsSet(chunk, code.A, attr.AType),
                            b,
                            c);
                    }
                    break;
                case JitOperand.TSETV:
                case JitOperand.TSETB:
                case JitOperand.TSETR:
                    {
                        builder.WriteFormat("{0}[{1}] = {2}",
                            TranslateNotSet(chunk, code.B, attr.BType),
                            TranslateNotSet(chunk, (int)cd, attr.CDType),
                            TranslateNotSet(chunk, code.A, attr.AType));
                    }
                    break;
                case JitOperand.TSETS:
                    builder.WriteFormat("{0}.{1} = {2}",
                        TranslateNotSet(chunk, code.B, attr.BType),
                        TranslateNotSet(chunk, (int)cd, attr.CDType),
                        TranslateNotSet(chunk, code.A, attr.AType));
                    break;
                case JitOperand.TSETM:
                    builder.WriteFormat("for i = 0, MULTRES, 1 do {0}[{1} + i] = slot({2} + i)",
                        TranslateNotSet(chunk, code.A - 1, JitOperandFormat.VAR),
                        cd,
                        code.A);
                    break;
                case JitOperand.CALLM:
                    {
                        var fn = TranslateNotSet(chunk, code.A, JitOperandFormat.VAR);
                        var begin = code.A + 1;
                        var count = (int)cd;
                        var args = new string[count];
                        for (int i = 0; i < count; i++)
                        {
                            args[i] = TranslateNotSet(chunk, begin + i, JitOperandFormat.VAR);
                        }
                        begin = code.A;
                        count = code.B - 1;
                        for (int i = 0; i < count; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(TranslateIsSet(chunk, begin + i, JitOperandFormat.DST));
                        }
                        builder.WriteFormat(" = {0}({1}, ...MULTRES)", fn, string.Join(", ", args));
                        break;
                    }
                case JitOperand.CALL:
                    {
                        var fn = TranslateNotSet(chunk, code.A, JitOperandFormat.VAR);
                        var begin = code.A + 1;
                        var count = code.B - 1;
                        var args = new string[count];
                        for (int i = 0; i < count; i++)
                        {
                            args[i] = TranslateNotSet(chunk, begin + i, JitOperandFormat.VAR);
                        }
                        if (cd > 1)
                        {
                            builder.Write(TranslateIsSet(chunk, code.A, JitOperandFormat.DST));
                            builder.Write(" = ");
                        }
                        builder.WriteFormat("{0}({1})", fn, string.Join(", ", args));
                        break;
                    }
                case JitOperand.CALLMT:
                    {
                        builder.Write("return ")
                            .Write(TranslateNotSet(chunk, code.A, JitOperandFormat.VAR))
                            .Write('(');
                        var count = cd;
                        for (int i = 0; i < count; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(TranslateNotSet(chunk, code.A + i + 1, JitOperandFormat.VAR));
                        }
                        builder.Write(", ...MULTRES)");
                        break;
                    }
                case JitOperand.CALLT:
                    {
                        builder.Write("return ")
                            .Write(TranslateNotSet(chunk, code.A, JitOperandFormat.VAR))
                            .Write('(');
                        var count = cd;
                        for (int i = 1; i < count; i++)
                        {
                            if (i > 1)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(TranslateNotSet(chunk, code.A + i, JitOperandFormat.VAR));
                        }
                        builder.Write(')');
                        break;
                    }
                case JitOperand.ITERC:
                case JitOperand.ITERN:
                    {
                        var A_minus_three = TranslateValue(chunk, code.A - 3, JitOperandFormat.VAR);
                        var A_minus_two = TranslateValue(chunk, code.A - 2, JitOperandFormat.VAR);
                        var A_minus_one = TranslateValue(chunk, code.A - 1, JitOperandFormat.VAR);
                        
                        builder.WriteFormat("{0}, {1}, {2} = {3}, {4}, {5}", 
                            TranslateValue(chunk, code.A, JitOperandFormat.DST),
                            TranslateValue(chunk, code.A + 1, JitOperandFormat.DST),
                            TranslateValue(chunk, code.A + 2, JitOperandFormat.DST),
                            A_minus_three,
                            A_minus_two,
                            A_minus_one).WriteLine(true);
                        for (int i = 0; i < code.B - 1; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(TranslateIsSet(chunk, code.A + i, JitOperandFormat.DST));
                        }
                        builder.WriteFormat(" = {0}({1}, {2})",
                            A_minus_three, A_minus_two, A_minus_one);
                        break;
                    }
                case JitOperand.VARG:
                    {
                        if (code.B - 2 < 0)
                        {
                            builder.Write("MULTRES");
                        } else
                        {
                            for (int i = 0; i < code.B - 2; i++)
                            {
                                if (i > 0)
                                {
                                    builder.Write(", ");
                                }
                                builder.Write(TranslateIsSet(chunk, code.A + i, JitOperandFormat.DST));
                            }
                        }
                        builder.Write(" = ...");
                        break;
                    }
                case JitOperand.ISNEXT:
                    {
                        var d = TranslateValue(chunk, (int)cd, JitOperandFormat.JMP);
                        builder.WriteFormat("Verify ITERN at {0}; goto {0}", d);
                        break;
                    }
                case JitOperand.RETM:
                    {
                        builder.Write("return ");
                        var count = (int)cd - 1;
                        for (int i = 0; i < count; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(TranslateNotSet(chunk, code.A + i, JitOperandFormat.VAR));
                        }
                        builder.Write(", ...MULTRES");
                        break;
                    }
                case JitOperand.RET:
                    {
                        builder.Write("return ");
                        var count = (int)cd - 2;
                        for (int i = 0; i < count; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(TranslateNotSet(chunk, code.A + i, JitOperandFormat.VAR));
                        }
                        break;
                    }
                case JitOperand.RET0:
                    if (builder.Indent == 0)
                    {
                        break;
                    }
                    builder.Write("return");
                    break;
                case JitOperand.RET1:
                    builder.Write("return ").Write(TranslateNotSet(chunk, code.A, attr.AType));
                    break;
                case JitOperand.FORI:
                case JitOperand.JFORI:
                    {
                        var result = TranslateIsSet(chunk, code.A + 3, JitOperandFormat.VAR);
                        builder.WriteFormat("for {0} = {1},{2},{3}", result,
                            TranslateValue(chunk, code.A, JitOperandFormat.BS),
                            TranslateValue(chunk, code.A + 1, JitOperandFormat.BS),
                            TranslateValue(chunk, code.A + 2, JitOperandFormat.BS))
                            .WriteLine(true)
                            .WriteFormat("else goto {0}", 
                            TranslateValue(chunk, (int)cd, JitOperandFormat.JMP));
                        break;
                    }
                case JitOperand.FORL:
                case JitOperand.IFORL:
                case JitOperand.JFORL:
                    {
                        var a = TranslateValue(chunk, code.A + 3, JitOperandFormat.VAR);
                        var two = TranslateValue(chunk, code.A + 2, JitOperandFormat.VAR);
                        builder.WriteFormat("{0} = {0} + {1}", a,
                            two)
                            .WriteLine(true)
                            .WriteFormat("if cmp({0}, sign {1}, {2}) goto {3}", 
                            a, two, TranslateValue(chunk, code.A + 1, JitOperandFormat.VAR),
                            TranslateValue(chunk, (int)cd, JitOperandFormat.JMP));
                        break;
                    }
                case JitOperand.ITERL:
                case JitOperand.IITERL:
                case JitOperand.JITERL:
                    {
                        var a = TranslateValue(chunk, code.A, JitOperandFormat.VAR);
                        builder.WriteFormat("{0} = {1}", 
                            TranslateValue(chunk, code.A - 1, JitOperandFormat.VAR),
                            a)
                            .WriteLine(true)
                            .WriteFormat("if {0} != nil goto {1}",
                            a,
                            TranslateValue(chunk, (int)cd, JitOperandFormat.JMP));
                        break;
                    }
                case JitOperand.LOOP:
                    builder.Write("Noop");
                    break;
                case JitOperand.ILOOP:
                    builder.Write("Noop");
                    break;
                case JitOperand.JLOOP:
                    builder.Write("Noop");
                    break;
                case JitOperand.JMP:
                    // TODO 暂时无法确定位置
                    builder.WriteFormat("    goto {0}", TranslateValue(chunk, (int)(cd - 0x8000), JitOperandFormat.JMP));
                    break;
                case JitOperand.FUNCF:
                    builder.Write("Fixed-arg function with frame size ")
                          .Write(TranslateValue(chunk, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.IFUNCF:
                    builder.Write("Interpreted fixed-arg function with frame size ")
                         .Write(TranslateValue(chunk, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.JFUNCF:
                    builder.Write("JIT compiled fixed-arg function with frame size ")
                         .Write(TranslateValue(chunk, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.FUNCV:
                    builder.Write("Var-arg function with frame size ")
                         .Write(TranslateValue(chunk, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.IFUNCV:
                    builder.Write("Interpreted var-arg function with frame size ")
                         .Write(TranslateValue(chunk, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.JFUNCV:
                    builder.Write("JIT compiled var-arg function with frame size ")
                         .Write(TranslateValue(chunk, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.FUNCC:
                    builder.Write("C function with frame size ")
                        .Write(TranslateValue(chunk, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.FUNCCW:
                    builder.Write("Wrapped C function with frame size ")
                        .Write(TranslateValue(chunk, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.UNKNW:
                    builder.Write("Unknown instruction");
                    break;
                default:
                    break;
            }
            
        }

        private string TranslateIsSet(LuaChunk chunk, int value, JitOperandFormat format)
        {
            var res = TranslateName(chunk, value, format);
            _envItems.Remove(res);
            if (format == JitOperandFormat.DST && TryGetLocal(chunk, value, out var fn))
            {
                return fn;
            }
            return res;
        }

        private string TranslateNotSet(LuaChunk chunk,
            int value,
            JitOperandFormat format)
        {
            var res = TranslateValue(chunk, value, format);
            if (format == JitOperandFormat.VAR)
            {
                if (_envItems.TryGetValue(res, out var fn))
                {
                    return fn;
                }
                if (TryGetLocal(chunk, value, out fn))
                {
                    return fn;
                }
            }
            return res;
        }

        private static string TranslateValue(LuaChunk chunk, int value,
            JitOperandFormat format)
        {
            if (format is JitOperandFormat.DST or JitOperandFormat.BS)
            {
                return $"slot{value}";
            }
            if (format == JitOperandFormat.VAR)
            {
                return /*GetLocalName(chunk, value) ?? */$"slot{value}";
            }
            if (format == JitOperandFormat.UV)
            {
                var name = chunk.DebugInfo.UpValueNameItems.Length > value ? 
                    chunk.DebugInfo.UpValueNameItems[value] : "unknwon";
                return $"uv{value}\"${name}\"";
            }
            if (format == JitOperandFormat.PRI)
            {
                return value switch
                {
                    0 => "nil",
                    2 => "true",
                    _ => "false",
                };
            }
            if (format == JitOperandFormat.NUM)
            {
                return chunk.NumberConstantItems.Length > value ? 
                    chunk.NumberConstantItems[value].Value.ToString() : "0";
            }
            if (format == JitOperandFormat.STR)
            {
                return $"\"{chunk.ConstantItems[value].Value}\"";
            }
            if (format == JitOperandFormat.TAB)
            {
                return $"table#k{value}";
            }
            if (format == JitOperandFormat.CDT)
            {
                return chunk.ConstantItems[value].Value.ToString();
            }
            if (format == JitOperandFormat.JMP)
            {
                var pc = value + 1;
                return (pc >= 0 ? "+" : string.Empty) + pc.ToString();// $"{1 + chunk.CurrentIndex + value}";
            }
            if (format is JitOperandFormat.LIT or JitOperandFormat.SLIT)
            {
                return value.ToString();
            }
            if (format is JitOperandFormat.BS or JitOperandFormat.RBS)
            {
                return $"r{value}";
            }
            return " ";
        }
        private static string TranslateName(LuaChunk chunk, int value,
            JitOperandFormat format)
        {
            if (format == JitOperandFormat.STR)
            {
                return $"{chunk.ConstantItems[value].Value}";
            }
            return TranslateValue(chunk, value, format);
        }
        private static string TranslateOperation(JitOperand op)
        {
            return op switch
            {
                JitOperand.ADDNV or JitOperand.ADDVV or JitOperand.ADDVN => "+",
                JitOperand.SUBNV or JitOperand.SUBVV or JitOperand.SUBVN => "-",
                JitOperand.MULNV or JitOperand.MULVV or JitOperand.MULVN => "*",
                JitOperand.DIVNV or JitOperand.DIVVV or JitOperand.DIVVN => "/",
                JitOperand.MODNV or JitOperand.MODVV or JitOperand.MODVN => "%",
                JitOperand.POW => "^",
                JitOperand.CAT => "..",

                JitOperand.UNM => "-",
                JitOperand.NOT => "not ",
                JitOperand.LEN => "#",

                JitOperand.ISEQN or JitOperand.ISEQP or JitOperand.ISEQS or JitOperand.ISEQV => "==",
                JitOperand.ISNEN or JitOperand.ISNEP or JitOperand.ISNES or JitOperand.ISNEV => "~=",
                JitOperand.ISLE => "<=",
                JitOperand.ISLT => "<",
                JitOperand.ISGT => ">",
                JitOperand.ISGE => ">=",
                _ => throw new NotImplementedException()
            };
        }
    }
}
