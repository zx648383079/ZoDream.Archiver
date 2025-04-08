using System;
using System.Linq;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Language;

namespace ZoDream.LuaDecompiler
{
    public partial class LuaWriter
    {

        private string[] Translate(LuaChunk chunk, OperandCode code)
        {
            var attr = OperandExtractor.GetAttribute(code.Operand);
            if (attr is null)
            {
                return [];
            }
            return [.. attr.Operands.Select(i => Translate(chunk, code, i))];
        }

        private string Translate(LuaChunk chunk, OperandCode code, OperandFormat format)
        {
            var attr = OperandExtractor.GetAttribute(format);
            if (attr is null)
            {
                return string.Empty;
            }
            var field = attr.Field switch
            {
                OperandField.A => code.Extractor.A,
                OperandField.B => code.Extractor.B,
                OperandField.C => code.Extractor.C,
                OperandField.k => code.Extractor.K,
                OperandField.Ax => code.Extractor.Ax,
                OperandField.sJ => code.Extractor.SJ,
                OperandField.Bx => code.Extractor.Bx,
                OperandField.sBx => code.Extractor.SBx,
                OperandField.x => code.Extractor.X,
                _ => throw new System.NotImplementedException(),
            };
            var val = field.Extract(code.Codepoint);
            return attr.Format switch
            {
                OperandFieldFormat.RAW or OperandFieldFormat.IMMEDIATE_INTEGER 
                or OperandFieldFormat.IMMEDIATE_FLOAT => val.ToString() ?? string.Empty,
                OperandFieldFormat.REGISTER => $"r{val}",
                OperandFieldFormat.UPVALUE => $"u{val}",
                OperandFieldFormat.REGISTER_K => code.Extractor.IsK(val) ? $"k{code.Extractor.DecodeK(val)}" : $"r{val}",
                OperandFieldFormat.REGISTER_K54 => code.K ? $"k{val}" : $"r{val}",
                OperandFieldFormat.CONSTANT or OperandFieldFormat.CONSTANT_INTEGER or OperandFieldFormat.CONSTANT_STRING => $"k{val}",
                OperandFieldFormat.FUNCTION => $"f{val}",
                OperandFieldFormat.IMMEDIATE_SIGNED_INTEGER => (val - field.Max / 2).ToString(),
                OperandFieldFormat.JUMP => (val + attr.Offset).ToString(),
                OperandFieldFormat.JUMP_NEGATIVE => (-val).ToString(),
                _ => throw new System.NotImplementedException(),
            };
        }

        private void Translate(ICodeWriter builder, LuaChunk chunk, int index, OperandCode code)
        {
            var fieldItems = Translate(chunk, code);
            switch (code.Operand)
            {
                case Operand.MOVE:
                    builder.Write(fieldItems[0]).Write(" = ").Write(fieldItems[1]);
                    break;
                case Operand.LOADI:
                    builder.Write(fieldItems[0]).Write(" = ").Write(fieldItems[1]);
                    break;
                case Operand.LOADF:
                    builder.Write(fieldItems[0]).Write(" = ").Write(fieldItems[1]);
                    break;
                case Operand.LOADK:
                    builder.Write(fieldItems[0]).Write(" = ").Write(fieldItems[1]);
                    break;
                case Operand.LOADBOOL:
                    builder.Write(fieldItems[0]).Write(" = ").Write(code.B != 0 ? "true" : "false");
                    break;
                case Operand.LOADFALSE:
                    builder.Write(fieldItems[0]).Write(" = false");
                    break;
                case Operand.LOADTRUE:
                    builder.Write(fieldItems[0]).Write(" = true");
                    break;
                
                case Operand.LOADKX:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(chunk.ConstantItems[code.Extractor.Ax.Extract(code.Codepoint + 1)]);
                    break;
                case Operand.LOADNIL:
                    for (int i = code.A; i <= code.B; i++)
                    {
                        builder.Write($"r{i}").Write(" = nil");
                    }
                    break;
                case Operand.LOADNIL52:
                    for (int i = 0; i <= code.B; i++)
                    {
                        builder.Write($"r{code.A + i}").Write(" = nil");
                    }
                    break;
                case Operand.GETUPVAL:
                    builder.Write(fieldItems[0]).Write(" = _ENV[").Write(fieldItems[1]).Write(']');
                    break;
                case Operand.GETGLOBAL:
                    builder.Write(fieldItems[0]).Write(" = _G[").Write(fieldItems[1]).Write(']');
                    break;
                case Operand.SETGLOBAL:
                    builder.Write("_G[").Write(fieldItems[0]).Write("] = ")
                        .Write(fieldItems[1]);
                    break;
                case Operand.SETUPVAL:
                    builder.Write("_ENV[").Write(fieldItems[0]).Write("] = ")
                        .Write(fieldItems[1]);
                    break;
                case Operand.SELF:
                    builder.Write($"r{code.A + 1}").Write(" = ")
                        .Write($"r{code.B}");
                    builder.Write($"r{code.A}").Write(" = ")
                        .Write($"r{code.C}"); // c table ref
                    break;
                case Operand.SELF54:
                    builder.Write($"r{code.A + 1}").Write(" = ")
                        .Write($"r{code.B}");
                    builder.Write($"r{code.A}").Write(" = ")
                        .Write($"r{code.C}"); // c table ref
                    break;
                case Operand.ADD:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(fieldItems[1]).Write(" + ").Write(fieldItems[2]);
                    break;
                case Operand.ADD54:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(chunk.DebugInfo.LocalItems[code.B]?.Name ?? fieldItems[1])
                        .Write(" + ")
                        .Write(chunk.DebugInfo.LocalItems[code.C]?.Name ?? fieldItems[2]);
                    break;
                case Operand.SUB:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(fieldItems[1]).Write(" - ").Write(fieldItems[2]);
                    break;
                case Operand.SUB54:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(chunk.DebugInfo.LocalItems[code.B]?.Name ?? fieldItems[1])
                        .Write(" - ")
                        .Write(chunk.DebugInfo.LocalItems[code.C]?.Name ?? fieldItems[2]);
                    break;
                case Operand.MUL:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(fieldItems[1]).Write(" * ").Write(fieldItems[2]);
                    break;
                case Operand.MUL54:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(chunk.DebugInfo.LocalItems[code.B]?.Name ?? fieldItems[1])
                        .Write(" * ")
                        .Write(chunk.DebugInfo.LocalItems[code.C]?.Name ?? fieldItems[2]);
                    break;
                case Operand.DIV:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(fieldItems[1]).Write(" / ").Write(fieldItems[2]);
                    break;
                case Operand.DIV54:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(chunk.DebugInfo.LocalItems[code.B]?.Name ?? fieldItems[1])
                        .Write(" / ")
                        .Write(chunk.DebugInfo.LocalItems[code.C]?.Name ?? fieldItems[2]);
                    break;
                case Operand.MOD:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(fieldItems[1]).Write(" % ").Write(fieldItems[2]);
                    break;
                case Operand.MOD54:
                    builder.Write(fieldItems[0]).Write(" = ")
                         .Write(chunk.DebugInfo.LocalItems[code.B]?.Name ?? fieldItems[1])
                         .Write(" % ")
                         .Write(chunk.DebugInfo.LocalItems[code.C]?.Name ?? fieldItems[2]);
                    break;
                case Operand.POW:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(fieldItems[1]).Write(" ^ ").Write(fieldItems[2]);
                    break;
                case Operand.POW54:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(chunk.DebugInfo.LocalItems[code.B]?.Name ?? fieldItems[1])
                        .Write(" ^ ")
                        .Write(chunk.DebugInfo.LocalItems[code.C]?.Name ?? fieldItems[2]);
                    break;
                case Operand.IDIV:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(fieldItems[1]).Write(" // ").Write(fieldItems[2]);
                    break;
                case Operand.IDIV54:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(chunk.DebugInfo.LocalItems[code.B]?.Name ?? fieldItems[1])
                        .Write(" // ")
                        .Write(chunk.DebugInfo.LocalItems[code.C]?.Name ?? fieldItems[2]);
                    break;
                case Operand.BAND:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(fieldItems[1]).Write(" & ").Write(fieldItems[2]);
                    break;
                case Operand.BAND54:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(chunk.DebugInfo.LocalItems[code.B]?.Name ?? fieldItems[1])
                        .Write(" & ")
                        .Write(chunk.DebugInfo.LocalItems[code.C]?.Name ?? fieldItems[2]);
                    break;
                case Operand.BOR:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(fieldItems[1]).Write(" | ").Write(fieldItems[2]);
                    break;
                case Operand.BOR54:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(chunk.DebugInfo.LocalItems[code.B]?.Name ?? fieldItems[1])
                        .Write(" | ")
                        .Write(chunk.DebugInfo.LocalItems[code.C]?.Name ?? fieldItems[2]);
                    break;
                case Operand.BXOR:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(fieldItems[1]).Write(" ~ ").Write(fieldItems[2]);
                    break;
                case Operand.BXOR54:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(chunk.DebugInfo.LocalItems[code.B]?.Name ?? fieldItems[1])
                        .Write(" ~ ")
                        .Write(chunk.DebugInfo.LocalItems[code.C]?.Name ?? fieldItems[2]);
                    break;
                case Operand.SHL:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(fieldItems[1]).Write(" << ").Write(fieldItems[2]);
                    break;
                case Operand.SHL54:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(chunk.DebugInfo.LocalItems[code.B]?.Name ?? fieldItems[1])
                        .Write(" << ")
                        .Write(chunk.DebugInfo.LocalItems[code.C]?.Name ?? fieldItems[2]);
                    break;
                case Operand.SHR:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(fieldItems[1]).Write(" >> ").Write(fieldItems[2]);
                    break;
                case Operand.SHR54:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(chunk.DebugInfo.LocalItems[code.B]?.Name ?? fieldItems[1])
                        .Write(" >> ")
                        .Write(chunk.DebugInfo.LocalItems[code.C]?.Name ?? fieldItems[2]);
                    break;
                case Operand.UNM:
                    builder.Write(fieldItems[0]).Write(" = -")
                        .Write(fieldItems[1]);
                    break;
                case Operand.NOT:
                    builder.Write(fieldItems[0]).Write(" = not ")
                        .Write(fieldItems[1]);
                    break;
                case Operand.LEN:
                    builder.Write(fieldItems[0]).Write(" = #")
                        .Write(fieldItems[1]);
                    break;
                case Operand.BNOT:
                    builder.Write(fieldItems[0]).Write(" = ~")
                        .Write(fieldItems[1]);
                    break;
                case Operand.CONCAT:
                    builder.Write(fieldItems[0]).Write(" = ");
                    for (var i = 0; i <= code.C - code.B; i++)
                    {
                        if (i > 0)
                        {
                            builder.Write(" .. ");
                        }
                        builder.Write($"r{code.B + i}");
                    }
                    break;
                case Operand.CONCAT54:
                    builder.Write(fieldItems[0]).Write(" = ");
                    for (var i = 0; i <= code.B; i++)
                    {
                        if (i > 0)
                        {
                            builder.Write(" .. ");
                        }
                        builder.Write($"r{code.A + i}");
                    }
                    break;
                case Operand.JMP:
                    builder.Write("goto ").Write(fieldItems[0]);
                    break;
                case Operand.EQ:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(fieldItems[1]).Write(" == ").Write(fieldItems[2]);
                    break;
                case Operand.LT:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(fieldItems[1]).Write(" < ").Write(fieldItems[2]);
                    break;
                case Operand.LE:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(fieldItems[1]).Write(" <= ").Write(fieldItems[2]);
                    break;
                case Operand.TEST:
                    break;
                case Operand.TEST54:
                    break;
                case Operand.TESTSET:
                    break;
                case Operand.TESTSET54:
                    break;
                case Operand.CALL:
                    {
                        var b = code.B == 0 ? code.Codepoint - code.A : code.B;
                        var c = code.C == 0 ? code.Codepoint - code.A + 1 : code.C;
                        if (c == 2)
                        {
                            if (code.C >= 3 || code.C == 0)
                            {
                                for (var i = 0; i <= b - 2; i++)
                                {
                                    if (i > 0)
                                    {
                                        builder.Write(", ");
                                    }
                                    builder.Write($"r{code.A + i}");
                                }
                            } else
                            {
                                builder.Write(fieldItems[0]);
                            }
                            builder.Write(" = ");
                        }
                        builder.Write(chunk.DebugInfo.LocalItems[code.A]?.Name ?? fieldItems[0])
                            .Write('(');
                        for (var i = 1; i < b; i++)
                        {
                            if (i > 1)
                            {
                                builder.Write(", ");
                            }
                            builder.Write($"r{code.A + i}");
                        }
                        builder.Write(')');
                        if (c == 1)
                        {
                            builder.Write("()");
                        }
                    }
                    break;
                case Operand.TAILCALL:
                case Operand.TAILCALL54:
                    {
                        var b = code.B == 0 ? code.Codepoint - code.A : code.B;
                        builder.Write("return ").Write(chunk.DebugInfo.LocalItems[code.A]?.Name ?? fieldItems[0])
                            .Write('(');
                        for (var i = 1; i < b; i++)
                        {
                            if (i > 1)
                            {
                                builder.Write(", ");
                            }
                            builder.Write($"r{code.A + i}");
                        }
                        builder.Write(')');
                    }
                    break;
                case Operand.RETURN:
                case Operand.RETURN54:
                    {
                        var b = code.B == 0 ? code.Codepoint - code.A + 1 : code.B;
                        builder.Write("return ");
                        for (var i = 0; i <= b - 2; i++)
                        {
                            if (i > 1)
                            {
                                builder.Write(", ");
                            }
                            builder.Write($"r{code.A + i}");
                        }
                    }
                    break;
                case Operand.RETURN0:
                    builder.Write("return 0");
                    break;
                case Operand.RETURN1:
                    builder.Write("return ").Write(fieldItems[0]);
                    break;
                case Operand.FORLOOP:
                    break;
                case Operand.FORPREP:
                    break;
                case Operand.TFORLOOP:
                    break;
                case Operand.SETLIST:
                    {
                        var stack = code.A;
                        var count = code.B == 0 ? code.Codepoint - code.A - 1 : code.B;
                        var offset = 
                            (code.C == 0 ? (chunk.OpcodeItems[index + 1] as OperandCode).Codepoint : code.C - 1) % 50;
                        for (int i = 1; i < count; i++)
                        {
                            builder.Write($"r{stack}[{offset + i}] = r{stack + i}");
                        }
                    }
                    break;
                case Operand.SETLIST50:
                    {
                        var stack = code.A;
                        var count = 1 + code.Bx % 32;
                        var offset = code.Bx - code.Bx % 32;
                        for (int i = 1; i < count; i++)
                        {
                            builder.Write($"r{stack}[{offset + i}] = r{stack + i}");
                        }
                    }
                    break;
                case Operand.SETLIST52:
                    {
                        var stack = code.A;
                        var count = 1 + code.Bx % 32;
                        var offset = (code.C == 0 ? (chunk.OpcodeItems[index + 1] as OperandCode).Codepoint : code.C - 1) % 50;
                        for (int i = 1; i < count; i++)
                        {
                            builder.Write($"r{stack}[{offset + i}] = r{stack + i}");
                        }
                    }
                    break;
                case Operand.SETLIST54:
                    {
                        var c = code.C;
                        if (code.K)
                        {
                            c += (chunk.OpcodeItems[index + 1] as OperandCode).Ax * (code.Extractor.C.Max + 1);
                        }
                        var stack = code.A;
                        var count = 1 + code.Bx % 32;
                        var offset = (c - 1) % 50;
                        for (int i = 1; i < count; i++)
                        {
                            builder.Write($"r{stack}[{offset + i}] = r{stack + i}");
                        }
                    }
                    break;
                case Operand.SETLISTO:
                    {
                        var stack = code.A;
                        var count = code.Codepoint - code.A - 1;
                        var offset = code.Bx - code.Bx % 32;
                        for (int i = 1; i < count; i++)
                        {
                            builder.Write($"r{stack}[{offset + i}] = r{stack + i}");
                        }
                    }
                    break;
                case Operand.CLOSE:
                    break;
                case Operand.CLOSURE:
                    builder.Write(fieldItems[0]).Write(" = ");
                    Decompile(builder, chunk.PrototypeItems[code.Bx]);
                    break;
                case Operand.VARARG:
                    {
                        var multiple = code.B != 2;
                        var b = code.B == 0 ? code.Codepoint - code.A + 1 : code.B;
                        for (int i = 0; i <= b - 2; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write($"r{code.A + i}");
                        }
                        builder.Write(" = ").Write(multiple ? "..." : "(...)");
                    }
                    break;
                case Operand.VARARG54:
                    {
                        var multiple = code.C != 2;
                        var c = code.C == 0 ? code.Codepoint - code.A + 1 : code.B;
                        for (int i = 0; i <= c - 2; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write($"r{code.A + i}");
                        }
                        builder.Write(" = ").Write(multiple ? "..." : "(...)");
                    }
                    break;
                case Operand.JMP52:
                    break;
                
                case Operand.GETTABUP:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(fieldItems[1]).Write("[").Write(code.C).Write("]");
                    break;
                case Operand.SETTABUP:
                    builder.Write(fieldItems[0]).Write("[").Write(code.B).Write("]").Write(" = ")
                        .Write(fieldItems[2]);
                    break;
                case Operand.TFORCALL:
                    break;
                case Operand.TFORLOOP52:
                    break;
                case Operand.EXTRAARG:
                    break;
                case Operand.GETTABLE:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(fieldItems[1]).Write("[").Write(fieldItems[2]).Write("]");
                    break;
       
                case Operand.SETTABLE:
                    break;
                case Operand.NEWTABLE:
                    break;
                case Operand.NEWTABLE50:
                    break;
                case Operand.TFORPREP:
                    break;
                case Operand.TEST50:
                    break;
                case Operand.LFALSESKIP:
                    break;
                
                case Operand.GETTABUP54:
                    break;
                case Operand.GETTABLE54:
                    break;
                case Operand.GETI:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(fieldItems[1]).Write("[").Write(code.C).Write("]");
                    break;
                case Operand.GETFIELD:
                    builder.Write(fieldItems[0]).Write(" = ")
                        .Write(fieldItems[1]).Write("[").Write(fieldItems[2]).Write("]");
                    break;
                case Operand.SETTABUP54:
                    break;
                case Operand.SETTABLE54:
                    break;
                case Operand.SETI:
                    builder.Write(fieldItems[0]).Write("[").Write(code.B).Write("]").Write(" = ")
                        .Write(fieldItems[2]);
                    break;
                case Operand.SETFIELD:
                    builder.Write(fieldItems[0]).Write("[").Write(fieldItems[1])
                        .Write("]").Write(" = ")
                        .Write(fieldItems[2]);
                    break;
                case Operand.NEWTABLE54:
                    break;
                
                case Operand.ADDI:
                    {
                        var op = code.Extractor.C.Extract(code.Codepoint + 1) switch
                        {
                            6 => "+",
                            7 => "-",
                            8 => "*",
                            9 => "/",
                            10 => "%",
                            11 => "^",
                            12 => "//",
                            13 => "&",
                            14 => "|",
                            15 => "~",
                            16 => "<<",
                            17 => ">>",
                            _ => throw new Exception()
                        };
                        var immediate = code.SC;
                        var swap = false;
                        if (code.Extractor.K.Extract(code.Codepoint + 1) != 0)
                        {
                            swap = true;
                        } else if (op == "-")
                        {
                            immediate = -immediate;
                        }
                        builder.Write(fieldItems[0])
                            .Write(" = ")
                            .Write(swap ? immediate : $"r{code.B}")
                            .Write(op)
                            .Write(!swap ? immediate : $"r{code.B}");
                    }
                    break;
                case Operand.ADDK:
                    {
                        var swap = code.Extractor.K.Extract(code.Codepoint + 1) != 0;
                        builder.Write(fieldItems[0])
                            .Write(" = ")
                            .Write(swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}")
                            .Write(" + ")
                            .Write(!swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}");
                    }
                    break;
                case Operand.SUBK:
                    {
                        var swap = code.Extractor.K.Extract(code.Codepoint + 1) != 0;
                        builder.Write(fieldItems[0])
                            .Write(" = ")
                            .Write(swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}")
                            .Write(" - ")
                            .Write(!swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}");
                    }
                    break;
                case Operand.MULK:
                    {
                        var swap = code.Extractor.K.Extract(code.Codepoint + 1) != 0;
                        builder.Write(fieldItems[0])
                            .Write(" = ")
                            .Write(swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}")
                            .Write(" * ")
                            .Write(!swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}");
                    }
                    break;
                case Operand.MODK:
                    {
                        var swap = code.Extractor.K.Extract(code.Codepoint + 1) != 0;
                        builder.Write(fieldItems[0])
                            .Write(" = ")
                            .Write(swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}")
                            .Write(" % ")
                            .Write(!swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}");
                    }
                    break;
                case Operand.POWK:
                    {
                        var swap = code.Extractor.K.Extract(code.Codepoint + 1) != 0;
                        builder.Write(fieldItems[0])
                            .Write(" = ")
                            .Write(swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}")
                            .Write(" ^ ")
                            .Write(!swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}");
                    }
                    break;
                case Operand.DIVK:
                    {
                        var swap = code.Extractor.K.Extract(code.Codepoint + 1) != 0;
                        builder.Write(fieldItems[0])
                            .Write(" = ")
                            .Write(swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}")
                            .Write(" / ")
                            .Write(!swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}");
                    }
                    break;
                case Operand.IDIVK:
                    {
                        var swap = code.Extractor.K.Extract(code.Codepoint + 1) != 0;
                        builder.Write(fieldItems[0])
                            .Write(" = ")
                            .Write(swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}")
                            .Write(" // ")
                            .Write(!swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}");
                    }
                    break;
                case Operand.BANDK:
                    {
                        var swap = code.Extractor.K.Extract(code.Codepoint + 1) != 0;
                        builder.Write(fieldItems[0])
                            .Write(" = ")
                            .Write(swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}")
                            .Write(" && ")
                            .Write(!swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}");
                    }
                    break;
                case Operand.BORK:
                    {
                        var swap = code.Extractor.K.Extract(code.Codepoint + 1) != 0;
                        builder.Write(fieldItems[0])
                            .Write(" = ")
                            .Write(swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}")
                            .Write(" | ")
                            .Write(!swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}");
                    }
                    break;
                case Operand.BXORK:
                    {
                        var swap = code.Extractor.K.Extract(code.Codepoint + 1) != 0;
                        builder.Write(fieldItems[0])
                            .Write(" = ")
                            .Write(swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}")
                            .Write(" ~ ")
                            .Write(!swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}");
                    }
                    break;
                case Operand.SHRI:
                    {
                        var swap = code.Extractor.K.Extract(code.Codepoint + 1) != 0;
                        builder.Write(fieldItems[0])
                            .Write(" = ")
                            .Write(swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}")
                            .Write(" >> ")
                            .Write(!swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}");
                    }
                    break;
                case Operand.SHLI:
                    {
                        var swap = code.Extractor.K.Extract(code.Codepoint + 1) != 0;
                        builder.Write(fieldItems[0])
                            .Write(" = ")
                            .Write(swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}")
                            .Write(" << ")
                            .Write(!swap ? chunk.ConstantItems[code.C].Value : $"r{code.B}");
                    }
                    break;
                case Operand.MMBIN:
                    break;
                case Operand.MMBINI:
                    break;
                case Operand.MMBINK:
                    break;
                
                case Operand.TBC:
                    break;
                case Operand.JMP54:
                    break;
                case Operand.EQ54:
                    break;
                case Operand.LT54:
                    break;
                case Operand.LE54:
                    break;
                case Operand.EQK:
                    break;
                case Operand.EQI:
                    break;
                case Operand.LTI:
                    break;
                case Operand.LEI:
                    break;
                case Operand.GTI:
                    break;
                case Operand.GEI:
                    break;
        
                case Operand.FORLOOP54:
                    break;
                case Operand.FORPREP54:
                    break;
                case Operand.TFORPREP54:
                    break;
                case Operand.TFORCALL54:
                    break;
                case Operand.TFORLOOP54:
                    break;
                case Operand.VARARGPREP:
                    break;
                case Operand.EXTRABYTE:
                    break;
                case Operand.DEFAULT:
                    break;
                case Operand.DEFAULT54:
                    break;
            }
        }
    }
}
