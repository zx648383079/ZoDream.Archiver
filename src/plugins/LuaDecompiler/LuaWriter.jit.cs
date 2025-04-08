using System;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Language;

namespace ZoDream.LuaDecompiler
{
    public partial class LuaWriter
    {
        private void Translate(ICodeWriter builder, LuaChunk chunk, int index, JitOperandCode code)
        {
            var attr = JitOperandExtractor.GetAttribute(code.Operand);
            if (attr is null)
            {
                return;
            }
            switch (code.Operand)
            {
                case JitOperand.ISLT:
                    builder.Write("if ").Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" < ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.ISGE:
                    builder.Write("if ").Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" >= ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.ISLE:
                    builder.Write("if ").Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" <= ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.ISGT:
                    builder.Write("if ").Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" > ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.ISEQV:
                    builder.Write("if ").Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" == ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.ISNEV:
                    builder.Write("if ").Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" ~= ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.ISEQS:
                    builder.Write("if ").Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" == ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.ISNES:
                    builder.Write("if ").Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" ~= ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.ISEQN:
                    builder.Write("if ").Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" == ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.ISNEN:
                    builder.Write("if ").Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" ~= ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.ISEQP:
                    builder.Write("if ").Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" == ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.ISNEP:
                    builder.Write("if ").Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" ~= ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.ISTC:
                    {
                        var d = Translate(chunk, index, (int)code.D, attr.CDType);
                        builder.Write(Translate(chunk, index, code.A, attr.AType)).Write(" = ")
                            .Write(d).Write("; if ").Write(d);
                        break;
                    }
                case JitOperand.ISFC:
                    {
                        var d = Translate(chunk, index, (int)code.D, attr.CDType);
                        builder.Write(Translate(chunk, index, code.A, attr.AType))
                            .Write(" = ").Write(d)
                            .Write("; if not ").Write(d);
                        break;
                    }
                case JitOperand.IST:
                    builder.Write("if ").Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.ISF:
                    builder.Write("if not ").Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.ISTYPE:
                    break;
                case JitOperand.ISNUM:
                    break;
                case JitOperand.MOV:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                         .Write(" = ")
                         .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.NOT:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = not ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.UNM:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = -")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.LEN:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = #")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.ADDVN:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ").Write(Translate(chunk, index, code.B, attr.BType))
                        .Write(" + ").Write(Translate(chunk, index, code.C, attr.CDType));
                    break;
                case JitOperand.SUBVN:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ").Write(Translate(chunk, index, code.B, attr.BType))
                        .Write(" - ").Write(Translate(chunk, index, code.C, attr.CDType));
                    break;
                case JitOperand.MULVN:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ").Write(Translate(chunk, index, code.B, attr.BType))
                        .Write(" * ").Write(Translate(chunk, index, code.C, attr.CDType));
                    break;
                case JitOperand.DIVVN:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ").Write(Translate(chunk, index, code.B, attr.BType))
                        .Write(" / ").Write(Translate(chunk, index, code.C, attr.CDType));
                    break;
                case JitOperand.MODVN:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ").Write(Translate(chunk, index, code.B, attr.BType))
                        .Write(" % ").Write(Translate(chunk, index, code.C, attr.CDType));
                    break;
                case JitOperand.ADDNV:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ").Write(Translate(chunk, index, code.C, attr.CDType))
                        .Write(" + ").Write(Translate(chunk, index, code.B, attr.BType));
                    break;
                case JitOperand.SUBNV:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ").Write(Translate(chunk, index, code.C, attr.CDType))
                        .Write(" - ").Write(Translate(chunk, index, code.B, attr.BType));
                    break;
                case JitOperand.MULNV:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ").Write(Translate(chunk, index, code.C, attr.CDType))
                        .Write(" * ").Write(Translate(chunk, index, code.B, attr.BType));
                    break;
                case JitOperand.DIVNV:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ").Write(Translate(chunk, index, code.C, attr.CDType))
                        .Write(" / ").Write(Translate(chunk, index, code.B, attr.BType));
                    break;
                case JitOperand.MODNV:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ").Write(Translate(chunk, index, code.C, attr.CDType))
                        .Write(" % ").Write(Translate(chunk, index, code.B, attr.BType));
                    break;
                case JitOperand.ADDVV:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ").Write(Translate(chunk, index, code.B, attr.BType))
                        .Write(" + ").Write(Translate(chunk, index, code.C, attr.CDType));
                    break;
                case JitOperand.SUBVV:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ").Write(Translate(chunk, index, code.B, attr.BType))
                        .Write(" - ").Write(Translate(chunk, index, code.C, attr.CDType));
                    break;
                case JitOperand.MULVV:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ").Write(Translate(chunk, index, code.B, attr.BType))
                        .Write(" * ").Write(Translate(chunk, index, code.C, attr.CDType));
                    break;
                case JitOperand.DIVVV:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ").Write(Translate(chunk, index, code.B, attr.BType))
                        .Write(" / ").Write(Translate(chunk, index, code.C, attr.CDType));
                    break;
                case JitOperand.MODVV:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ").Write(Translate(chunk, index, code.B, attr.BType))
                        .Write(" % ").Write(Translate(chunk, index, code.C, attr.CDType));
                    break;
                case JitOperand.POW:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ").Write(Translate(chunk, index, code.B, attr.BType))
                        .Write(" ^ ").Write(Translate(chunk, index, code.C, attr.CDType))
                        .Write(" (pow)");
                    break;
                case JitOperand.CAT:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ");
                    for (var i = 0; i <= code.D - code.B; i++)
                    {
                        if (i > 0)
                        {
                            builder.Write(", ");
                        }
                        builder.Write(Translate(chunk, index, code.B + i, JitOperandFormat.VAR));
                    }
                    break;
                case JitOperand.KSTR:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.KCDATA:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                         .Write(" = ")
                         .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.KSHORT:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                         .Write(" = ")
                         .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.KNUM:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.KPRI:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.KNIL:
                    {
                        for (var i = code.A; i <= code.D; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(
                                Translate(chunk, index, i, JitOperandFormat.VAR)
                             );
                        }
                        builder.Write(" = nil");
                        break;
                    }
                case JitOperand.UGET:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.USETV:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.USETS:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.USETN:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.USETP:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.UCLO:
                    builder.Write("nil uvs >= ").Write(Translate(chunk, index, code.A, attr.AType))
                        .Write("; goto ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.FNEW:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = function ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.TNEW:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = new table( array: ")
                        .Write(code.D & 0b0000011111111111)
                        .Write(", dict: ").Write(Math.Pow(2, code.D >> 11)).Write(')');
                    break;
                case JitOperand.TDUP:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = copy ")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType));
                    break;
                case JitOperand.GGET:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = _env[")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType))
                        .Write("]");
                    break;
                case JitOperand.GSET:
                    builder.Write("_env[")
                        .Write(Translate(chunk, index, (int)code.D, attr.CDType))
                        .Write("]")
                        .Write(" = ")
                        .Write(Translate(chunk, index, code.A, attr.AType));
                    break;
                case JitOperand.TGETV:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ")
                        .Write(Translate(chunk, index, code.B, attr.BType))
                        .Write("[").Write(Translate(chunk, index, code.C, attr.CDType))
                        .Write("]");
                    break;
                case JitOperand.TGETS:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ")
                        .Write(Translate(chunk, index, code.B, attr.BType))
                        .Write(".").Write(Translate(chunk, index, code.C, attr.CDType));
                    break;
                case JitOperand.TGETB:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ")
                        .Write(Translate(chunk, index, code.B, attr.BType))
                        .Write("[").Write(Translate(chunk, index, code.C, attr.CDType))
                        .Write("]");
                    break;
                case JitOperand.TGETR:
                    builder.Write(Translate(chunk, index, code.A, attr.AType))
                        .Write(" = ")
                        .Write(Translate(chunk, index, code.B, attr.BType))
                        .Write("[").Write(Translate(chunk, index, code.C, attr.CDType))
                        .Write("]");
                    break;
                case JitOperand.TSETV:
                    builder.Write(Translate(chunk, index, code.B, attr.BType))
                        .Write("[").Write(Translate(chunk, index, code.C, attr.CDType))
                        .Write("] = ").Write(Translate(chunk, index, code.A, attr.AType));
                    break;
                case JitOperand.TSETS:
                    builder.Write(Translate(chunk, index, code.B, attr.BType))
                        .Write(".").Write(Translate(chunk, index, code.C, attr.CDType))
                        .Write(" = ").Write(Translate(chunk, index, code.A, attr.AType));
                    break;
                case JitOperand.TSETB:
                    builder.Write(Translate(chunk, index, code.B, attr.BType))
                        .Write("[").Write(Translate(chunk, index, code.C, attr.CDType))
                        .Write("] = ").Write(Translate(chunk, index, code.A, attr.AType));
                    break;
                case JitOperand.TSETM:
                    builder.Write("for i = 0, MULTRES, 1 do ")
                        .Write(Translate(chunk, index, code.A - 1, JitOperandFormat.VAR))
                        .Write('[').Write(code.D)
                        .Write(" + i] = slot(").Write(code.A).Write(" + i)");
                    break;
                case JitOperand.TSETR:
                    builder.Write(Translate(chunk, index, code.B, attr.BType))
                        .Write("[").Write(Translate(chunk, index, code.C, attr.CDType))
                        .Write("] = ").Write(Translate(chunk, index, code.A, attr.AType));
                    break;
                case JitOperand.CALLM:
                    {
                        for (int i = 0; i < code.B - 1; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(Translate(chunk, index, code.A + i, JitOperandFormat.DST));
                        }
                        builder.Write(" = ").Write(Translate(chunk, index, code.A, JitOperandFormat.VAR))
                            .Write('(');
                        for (int i = 0; i < code.D; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(Translate(chunk, index, code.A + i, JitOperandFormat.VAR));
                        }
                        builder.Write(", ...MULTRES)");
                        break;
                    }
                case JitOperand.CALL:
                    {
                        for (int i = 0; i < code.B - 1; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(Translate(chunk, index, code.A + i, JitOperandFormat.DST));
                        }
                        builder.Write(" = ").Write(Translate(chunk, index, code.A, JitOperandFormat.VAR))
                            .Write('(');
                        for (int i = 0; i < code.D - 1; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(Translate(chunk, index, code.A + i, JitOperandFormat.VAR));
                        }
                        builder.Write(')');
                        break;
                    }
                case JitOperand.CALLMT:
                    {
                        builder.Write("return ")
                            .Write(Translate(chunk, index, code.A, JitOperandFormat.VAR))
                            .Write('(');
                        for (int i = 0; i < code.D - 1; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(Translate(chunk, index, code.A + i, JitOperandFormat.VAR));
                        }
                        builder.Write(", ...MULTRES)");
                        break;
                    }
                case JitOperand.CALLT:
                    {
                        builder.Write("return ")
                            .Write(Translate(chunk, index, code.A, JitOperandFormat.VAR))
                            .Write('(');
                        for (int i = 0; i < code.D - 1; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(Translate(chunk, index, code.A + i, JitOperandFormat.VAR));
                        }
                        builder.Write(')');
                        break;
                    }
                case JitOperand.ITERC:
                case JitOperand.ITERN:
                    {
                        var A_minus_three = Translate(chunk, index, code.A - 3, JitOperandFormat.VAR);
                        var A_minus_two = Translate(chunk, index, code.A - 2, JitOperandFormat.VAR);
                        var A_minus_one = Translate(chunk, index, code.A - 1, JitOperandFormat.VAR);
                        
                        builder.Write(Translate(chunk, index, code.A, JitOperandFormat.DST))
                            .Write(", ").Write(Translate(chunk, index, code.A + 1, JitOperandFormat.DST))
                            .Write(", ").Write(Translate(chunk, index, code.A + 2, JitOperandFormat.DST))
                            .Write(" = ")
                            .Write(A_minus_three).Write(", ").Write(A_minus_two).Write(", ").Write(A_minus_one)
                            .Write("; ");
                        for (int i = 0; i < code.B - 1; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(Translate(chunk, index, code.A + i, JitOperandFormat.DST));
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
                                builder.Write(Translate(chunk, index, code.A + i, JitOperandFormat.DST));
                            }
                        }
                        builder.Write(" = ...");
                        break;
                    }
                case JitOperand.ISNEXT:
                    {
                        var d = Translate(chunk, index, (int)code.D, JitOperandFormat.JMP);
                        builder.Write("Verify ITERN at ").Write(d)
                            .Write("; goto ").Write(d);
                        break;
                    }
                case JitOperand.RETM:
                    {
                        builder.Write("return ");
                        for (int i = 0; i < code.D - 1; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(Translate(chunk, index, code.A + i, JitOperandFormat.VAR));
                        }
                        builder.Write(", ...MULTRES");
                        break;
                    }
                case JitOperand.RET:
                    {
                        builder.Write("return ");
                        for (int i = 0; i < code.D - 2; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(Translate(chunk, index, code.A + i, JitOperandFormat.VAR));
                        }
                        break;
                    }
                case JitOperand.RET0:
                    builder.Write("return");
                    break;
                case JitOperand.RET1:
                    builder.Write("return ").Write(Translate(chunk, index, code.A, attr.AType));
                    break;
                case JitOperand.FORI:
                case JitOperand.JFORI:
                    {
                        var result = Translate(chunk, index, code.A + 3, JitOperandFormat.VAR);
                        builder.Write("for ").Write(result)
                            .Write(" = ").Write(Translate(chunk, index, code.A, JitOperandFormat.BS))
                            .Write(",").Write(Translate(chunk, index, code.A + 1, JitOperandFormat.BS))
                            .Write(",").Write(Translate(chunk, index, code.A + 2, JitOperandFormat.BS))
                            .Write(") goto ")
                            .Write(Translate(chunk, index, (int)code.D, JitOperandFormat.JMP));
                        break;
                    }
                case JitOperand.FORL:
                case JitOperand.IFORL:
                case JitOperand.JFORL:
                    {
                        var a = Translate(chunk, index, code.A + 3, JitOperandFormat.VAR);
                        var two = Translate(chunk, index, code.A + 2, JitOperandFormat.VAR);
                        builder.Write(a)
                            .Write(" = ").Write(a)
                            .Write(" + ").Write(two)
                            .Write("; if cmp(").Write(a)
                            .Write(", sign ").Write(two)
                            .Write(", ")
                            .Write(Translate(chunk, index, code.A + 1, JitOperandFormat.VAR))
                            .Write(") goto ")
                            .Write(Translate(chunk, index, (int)code.D, JitOperandFormat.JMP));
                        break;
                    }
                case JitOperand.ITERL:
                case JitOperand.IITERL:
                case JitOperand.JITERL:
                    {
                        var a = Translate(chunk, index, code.A, JitOperandFormat.VAR);
                        builder.Write(Translate(chunk, index, code.A - 1, JitOperandFormat.VAR))
                            .Write(" = ").Write(a)
                            .Write("; if ").Write(a)
                            .Write(" != nil goto ")
                            .Write(Translate(chunk, index, (int)code.D,
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
                          .Write(Translate(chunk, index, (int)code.D, JitOperandFormat.JMP));
                    break;
                case JitOperand.FUNCF:
                    builder.Write("Fixed-arg function with frame size ")
                          .Write(Translate(chunk, index, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.IFUNCF:
                    builder.Write("Interpreted fixed-arg function with frame size ")
                         .Write(Translate(chunk, index, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.JFUNCF:
                    builder.Write("JIT compiled fixed-arg function with frame size ")
                         .Write(Translate(chunk, index, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.FUNCV:
                    builder.Write("Var-arg function with frame size ")
                         .Write(Translate(chunk, index, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.IFUNCV:
                    builder.Write("Interpreted var-arg function with frame size ")
                         .Write(Translate(chunk, index, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.JFUNCV:
                    builder.Write("JIT compiled var-arg function with frame size ")
                         .Write(Translate(chunk, index, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.FUNCC:
                    builder.Write("C function with frame size ")
                        .Write(Translate(chunk, index, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.FUNCCW:
                    builder.Write("Wrapped C function with frame size ")
                        .Write(Translate(chunk, index, code.A, JitOperandFormat.RBS));
                    break;
                case JitOperand.UNKNW:
                    builder.Write("Unknown instruction");
                    break;
                default:
                    break;
            }
            
        }

        private string Translate(LuaChunk chunk,
            int index, int value,
            JitOperandFormat format)
        {
            if (format is JitOperandFormat.DST or JitOperandFormat.BS)
            {
                return $"slot{value}";
            }
            if (format == JitOperandFormat.VAR)
            {
                return $"slot{value}";
            }
            if (format == JitOperandFormat.UV)
            {
                var name = chunk.DebugInfo.UpValueNameItems[value];
                return $"slot{value}\"${name}\"";
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
                return chunk.NumberConstantItems[value].Value.ToString();
            }
            if (format == JitOperandFormat.STR)
            {
                return $"\"{chunk.NumberConstantItems[value].Value}\"";
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
                return $"{1 + index + value}";
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
    }
}
