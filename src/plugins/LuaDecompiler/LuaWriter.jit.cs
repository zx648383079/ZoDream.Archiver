using System;
using ZoDream.LuaDecompiler.Models;
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
                        Translate(chunk, code.A, attr.AType),
                        TranslateOperation(code.Operand),
                        Translate(chunk, (int)cd, attr.CDType)
                        );
                    break;
                case JitOperand.ISTC:
                    {
                        var d = Translate(chunk, (int)cd, attr.CDType);
                        builder.Write(Translate(chunk, code.A, attr.AType)).Write(" = ")
                            .Write(d).Write("; if ").Write(d);
                        break;
                    }
                case JitOperand.ISFC:
                    {
                        var d = Translate(chunk, (int)cd, attr.CDType);
                        builder.Write(Translate(chunk, code.A, attr.AType))
                            .Write(" = ").Write(d)
                            .Write("; if not ").Write(d);
                        break;
                    }
                case JitOperand.IST:
                    builder.Write("if ").Write(Translate(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.ISF:
                    builder.Write("if not ").Write(Translate(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.ISTYPE:
                    break;
                case JitOperand.ISNUM:
                    break;
                case JitOperand.MOV:
                    builder.WriteFormat("{0} = {1}",
                        Translate(chunk, code.A, attr.AType),
                        TranslateNotSet(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.NOT:
                case JitOperand.UNM:
                case JitOperand.LEN:
                    builder.WriteFormat("{0} = {1}{2}", 
                        Translate(chunk, code.A, attr.AType),
                        TranslateOperation(code.Operand),
                        Translate(chunk, (int)cd, attr.CDType)
                        );
                    break;
                case JitOperand.ADDVN:
                case JitOperand.SUBVN:
                case JitOperand.MULVN:
                case JitOperand.DIVVN:
                case JitOperand.MODVN:
                    builder.WriteFormat("{0} = {1} {2} {3}",
                        Translate(chunk, code.A, attr.AType),
                        TranslateNotSet(chunk, (int)cd, attr.BType),
                        TranslateOperation(code.Operand),
                        TranslateNotSet(chunk, code.B, attr.CDType));
                    break;
                case JitOperand.ADDVV:
                case JitOperand.SUBVV:
                case JitOperand.MULVV:
                case JitOperand.DIVVV:
                case JitOperand.MODVV:
                    builder.WriteFormat("{0} = {1} {2} {3}",
                        Translate(chunk, code.A, attr.AType),
                        TranslateNotSet(chunk, code.B, attr.BType),
                        TranslateOperation(code.Operand),
                        TranslateNotSet(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.ADDNV:
                case JitOperand.SUBNV:
                case JitOperand.MULNV:
                case JitOperand.DIVNV:
                case JitOperand.MODNV:
                    builder.WriteFormat("{0} = {1} {2} {3}",
                        Translate(chunk, code.A, attr.AType),
                        TranslateNotSet(chunk, (int)cd, attr.CDType),
                        TranslateOperation(code.Operand),
                        TranslateNotSet(chunk, code.B, attr.BType));
                    break;
                case JitOperand.POW:
                    builder.Write(Translate(chunk, code.A, attr.AType))
                        .Write(" = ").Write(TranslateNotSet(chunk, code.B, attr.BType))
                        .Write(" ^ ").Write(TranslateNotSet(chunk, (int)cd, attr.CDType))
                        .Write(" (pow)");
                    break;
                case JitOperand.CAT:
                    {
                        builder.Write(Translate(chunk, code.A, attr.AType))
                            .Write(" = ");
                        var count = (int)cd - code.B;
                        for (int i = 0; i <= count; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(Translate(chunk, code.B + i, JitOperandFormat.VAR));
                        }
                    }
                    break;
                case JitOperand.KSTR:
                    builder.Write(Translate(chunk, code.A, attr.AType))
                        .Write(" = ")
                        .Write(Translate(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.KCDATA:
                    builder.Write(Translate(chunk, code.A, attr.AType))
                         .Write(" = ")
                         .Write(Translate(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.KSHORT:
                    builder.Write(Translate(chunk, code.A, attr.AType))
                         .Write(" = ")
                         .Write(Translate(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.KNUM:
                    builder.Write(Translate(chunk, code.A, attr.AType))
                        .Write(" = ")
                        .Write(Translate(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.KPRI:
                    builder.Write(Translate(chunk, code.A, attr.AType))
                        .Write(" = ")
                        .Write(Translate(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.KNIL:
                    {
                        for (var i = code.A; i <= cd; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(
                                Translate(chunk, i, JitOperandFormat.VAR)
                             );
                        }
                        builder.Write(" = nil");
                        break;
                    }
                case JitOperand.UGET:
                    builder.Write(Translate(chunk, code.A, attr.AType))
                        .Write(" = ")
                        .Write(TranslateNotSet(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.USETV:
                    builder.Write(Translate(chunk, code.A, attr.AType))
                        .Write(" = ")
                        .Write(TranslateNotSet(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.USETS:
                    builder.Write(Translate(chunk, code.A, attr.AType))
                        .Write(" = ")
                        .Write(TranslateNotSet(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.USETN:
                    builder.Write(Translate(chunk, code.A, attr.AType))
                        .Write(" = ")
                        .Write(TranslateNotSet(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.USETP:
                    builder.Write(Translate(chunk, code.A, attr.AType))
                        .Write(" = ")
                        .Write(TranslateNotSet(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.UCLO:
                    builder.Write("nil uvs >= ").Write(Translate(chunk, code.A, attr.AType))
                        .Write("; goto ")
                        .Write(Translate(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.FNEW:
                    builder.WriteFormat("function {0}()", Translate(chunk, code.A, attr.AType)).WriteLine()
                        .WriteIncIndent().WriteFormat(";{0} {1} FNEW {2} {3};",
                                chunk.CurrentIndex,
                                chunk.DebugInfo.LineNoItems.Length > chunk.CurrentIndex ?
                                chunk.DebugInfo.LineNoItems[chunk.CurrentIndex] : 0,
                                code.A, cd);
                    Decompile(builder, chunk.PrototypeItems[(int)chunk.ConstantItems[cd].Value]);
                    builder.WriteOutdent().Write("end");
                    break;
                case JitOperand.TNEW:
                    builder.Write(Translate(chunk, code.A, attr.AType))
                        .Write(" = new table( array: ")
                        .Write(cd & 0b0000011111111111)
                        .Write(", dict: ").Write(Math.Pow(2, cd >> 11)).Write(')');
                    break;
                case JitOperand.TDUP:
                    builder.Write(Translate(chunk, code.A, attr.AType))
                        .Write(" = copy ")
                        .Write(Translate(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.GGET:
                    //if (attr.CDType == JitOperandFormat.STR)
                    //{
                    //    _envItems[Translate(chunk, index, code.A, attr.AType)] = chunk.ConstantItems[cd].Value.ToString();
                    //    break;
                    //}
                    builder.WriteFormat("{0} = _env[{1}]",
                        Translate(chunk, code.A, attr.AType),
                        Translate(chunk, (int)cd, attr.CDType)
                    );
                    break;
                case JitOperand.GSET:
                    builder.WriteFormat("_env[{0}] = {1}",
                        Translate(chunk, (int)cd, attr.CDType),
                        TranslateNotSet(chunk, code.A, attr.AType));
                    break;
                case JitOperand.TGETV:
                    builder.Write(Translate(chunk, code.A, attr.AType))
                        .Write(" = ")
                        .Write(Translate(chunk, code.B, attr.BType))
                        .Write("[").Write(Translate(chunk, (int)cd, attr.CDType))
                        .Write("]");
                    break;
                case JitOperand.TGETS:
                    builder.Write(Translate(chunk, code.A, attr.AType))
                        .Write(" = ")
                        .Write(Translate(chunk, code.B, attr.BType))
                        .Write(".").Write(Translate(chunk, (int)cd, attr.CDType));
                    break;
                case JitOperand.TGETB:
                    builder.Write(Translate(chunk, code.A, attr.AType))
                        .Write(" = ")
                        .Write(Translate(chunk, (int)cd, attr.BType))
                        .Write("[").Write(Translate(chunk, code.B, attr.CDType))
                        .Write("]");
                    break;
                case JitOperand.TGETR:
                    builder.Write(Translate(chunk, code.A, attr.AType))
                        .Write(" = ")
                        .Write(Translate(chunk, code.B, attr.BType))
                        .Write("[").Write(Translate(chunk, (int)cd, attr.CDType))
                        .Write("]");
                    break;
                case JitOperand.TSETV:
                    builder.Write(Translate(chunk, code.B, attr.BType))
                        .Write("[").Write(Translate(chunk, (int)cd, attr.CDType))
                        .Write("] = ").Write(Translate(chunk, code.A, attr.AType));
                    break;
                case JitOperand.TSETS:
                    builder.Write(Translate(chunk, code.B, attr.BType))
                        .Write(".").Write(Translate(chunk, (int)cd, attr.CDType))
                        .Write(" = ").Write(TranslateNotSet(chunk, code.A, attr.AType));
                    break;
                case JitOperand.TSETB:
                    builder.Write(Translate(chunk, code.B, attr.BType))
                        .Write("[").Write(Translate(chunk, (int)cd, attr.CDType))
                        .Write("] = ").Write(TranslateNotSet(chunk, code.A, attr.AType));
                    break;
                case JitOperand.TSETM:
                    builder.Write("for i = 0, MULTRES, 1 do ")
                        .Write(Translate(chunk, code.A - 1, JitOperandFormat.VAR))
                        .Write('[').Write(cd)
                        .Write(" + i] = slot(").Write(code.A).Write(" + i)");
                    break;
                case JitOperand.TSETR:
                    builder.Write(Translate(chunk, code.B, attr.BType))
                        .Write("[").Write(Translate(chunk, (int)cd, attr.CDType))
                        .Write("] = ").Write(TranslateNotSet(chunk, code.A, attr.AType));
                    break;
                case JitOperand.CALLM:
                    {
                        for (int i = 0; i < code.B - 1; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(Translate(chunk, code.A + i, JitOperandFormat.DST));
                        }
                        builder.Write(" = ").Write(TranslateNotSet(chunk, code.A, JitOperandFormat.VAR))
                            .Write('(');
                        for (int i = 0; i < cd; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(Translate(chunk, code.A + i + 1, JitOperandFormat.VAR));
                        }
                        builder.Write(", ...MULTRES)");
                        break;
                    }
                case JitOperand.CALL:
                    {
                        if (cd > 1)
                        {
                            builder.Write(Translate(chunk, code.A, JitOperandFormat.DST));
                            builder.Write(" = ");
                        }
                        builder.Write(TranslateNotSet(chunk, code.A, JitOperandFormat.VAR))
                            .Write('(');
                        var count = code.B - 1;
                        for (int i = 0; i < count; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(Translate(chunk, code.A + i + 1, JitOperandFormat.VAR));
                        }
                        builder.Write(')');
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
                            builder.Write(Translate(chunk, code.A + i + 1, JitOperandFormat.VAR));
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
                        for (int i = 0; i < count; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(Translate(chunk, code.A + i, JitOperandFormat.VAR));
                        }
                        builder.Write(')');
                        break;
                    }
                case JitOperand.ITERC:
                case JitOperand.ITERN:
                    {
                        var A_minus_three = Translate(chunk, code.A - 3, JitOperandFormat.VAR);
                        var A_minus_two = Translate(chunk, code.A - 2, JitOperandFormat.VAR);
                        var A_minus_one = Translate(chunk, code.A - 1, JitOperandFormat.VAR);
                        
                        builder.Write(Translate(chunk, code.A, JitOperandFormat.DST))
                            .Write(", ").Write(Translate(chunk, code.A + 1, JitOperandFormat.DST))
                            .Write(", ").Write(Translate(chunk, code.A + 2, JitOperandFormat.DST))
                            .Write(" = ")
                            .Write(A_minus_three).Write(", ").Write(A_minus_two).Write(", ").Write(A_minus_one)
                            .Write("; ");
                        for (int i = 0; i < code.B - 1; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(Translate(chunk, code.A + i, JitOperandFormat.DST));
                        }
                        builder.Write(" = ")
                            .Write(A_minus_three)
                            .Write('(').Write(A_minus_two).Write(", ").Write(A_minus_one).Write(')');
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
                                builder.Write(Translate(chunk, code.A + i, JitOperandFormat.DST));
                            }
                        }
                        builder.Write(" = ...");
                        break;
                    }
                case JitOperand.ISNEXT:
                    {
                        var d = Translate(chunk, (int)cd, JitOperandFormat.JMP);
                        builder.Write("Verify ITERN at ").Write(d)
                            .Write("; goto ").Write(d);
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
                            builder.Write(Translate(chunk, code.A + i, JitOperandFormat.VAR));
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
                            builder.Write(Translate(chunk, code.A + i, JitOperandFormat.VAR));
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
                        var result = Translate(chunk, code.A + 3, JitOperandFormat.VAR);
                        builder.Write("for ").Write(result)
                            .Write(" = ").Write(Translate(chunk, code.A, JitOperandFormat.BS))
                            .Write(",").Write(Translate(chunk, code.A + 1, JitOperandFormat.BS))
                            .Write(",").Write(Translate(chunk, code.A + 2, JitOperandFormat.BS))
                            .Write(") goto ")
                            .Write(Translate(chunk, (int)cd, JitOperandFormat.JMP));
                        break;
                    }
                case JitOperand.FORL:
                case JitOperand.IFORL:
                case JitOperand.JFORL:
                    {
                        var a = Translate(chunk, code.A + 3, JitOperandFormat.VAR);
                        var two = Translate(chunk, code.A + 2, JitOperandFormat.VAR);
                        builder.Write(a)
                            .Write(" = ").Write(a)
                            .Write(" + ").Write(two)
                            .Write("; if cmp(").Write(a)
                            .Write(", sign ").Write(two)
                            .Write(", ")
                            .Write(Translate(chunk, code.A + 1, JitOperandFormat.VAR))
                            .Write(") goto ")
                            .Write(Translate(chunk, (int)cd, JitOperandFormat.JMP));
                        break;
                    }
                case JitOperand.ITERL:
                case JitOperand.IITERL:
                case JitOperand.JITERL:
                    {
                        var a = Translate(chunk, code.A, JitOperandFormat.VAR);
                        builder.Write(Translate(chunk, code.A - 1, JitOperandFormat.VAR))
                            .Write(" = ").Write(a)
                            .Write("; if ").Write(a)
                            .Write(" != nil goto ")
                            .Write(Translate(chunk, (int)cd,
                            //code.Operand == JitOperand.JITERL ? JitOperandFormat.LIT : 
                            JitOperandFormat.JMP));
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
                    builder.Write("    goto ")
                          .Write(Translate(chunk, (int)cd, JitOperandFormat.JMP));
                    break;
                case JitOperand.FUNCF:
                    builder.Write("Fixed-arg function with frame size ")
                          .Write(Translate(chunk, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.IFUNCF:
                    builder.Write("Interpreted fixed-arg function with frame size ")
                         .Write(Translate(chunk, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.JFUNCF:
                    builder.Write("JIT compiled fixed-arg function with frame size ")
                         .Write(Translate(chunk, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.FUNCV:
                    builder.Write("Var-arg function with frame size ")
                         .Write(Translate(chunk, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.IFUNCV:
                    builder.Write("Interpreted var-arg function with frame size ")
                         .Write(Translate(chunk, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.JFUNCV:
                    builder.Write("JIT compiled var-arg function with frame size ")
                         .Write(Translate(chunk, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.FUNCC:
                    builder.Write("C function with frame size ")
                        .Write(Translate(chunk, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.FUNCCW:
                    builder.Write("Wrapped C function with frame size ")
                        .Write(Translate(chunk, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.UNKNW:
                    builder.Write("Unknown instruction");
                    break;
                default:
                    break;
            }
            
        }
        private string TranslateNotSet(LuaChunk chunk,
            int value,
            JitOperandFormat format)
        {
            var res = Translate(chunk, value, format);
            if (format == JitOperandFormat.VAR && _envItems.TryGetValue(res, out var fn))
            {
                return fn;
            }
            return res;
        }
        private string Translate(LuaChunk chunk, int value,
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
                return $"{1 + chunk.CurrentIndex + value}";
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
