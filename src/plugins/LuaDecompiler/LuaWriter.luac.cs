using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Language;

namespace ZoDream.LuaDecompiler
{
    public partial class LuaWriter
    {
        private void Translate(ICodeWriter builder, LuaChunk chunk, OperandCode code)
        {
            switch (code.Operand)
            {
                case Operand.MOVE:
                    {
                        var key = TranslateValue(chunk, code.B, OperandFormat.REGISTER);
                        if (Contains(key))
                        {
                            Rename(key, TranslateName(chunk, code.A, OperandFormat.REGISTER));
                            break;
                        }
                        var val = TranslateNotSet(chunk, code.B, OperandFormat.REGISTER);
                        Add(TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                            val);
                        //builder.WriteFormat("{0} := {1}",
                        //    TranslateName(chunk, code.A, OperandFormat.REGISTER),
                        //    TranslateValue(chunk, code.B, OperandFormat.REGISTER)
                        //    );
                    }
                    break;
                case Operand.LOADI:
                    builder.WriteFormat("{0} := {1}",
                        TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                        code.SBx
                        );
                    break;
                case Operand.LOADF:
                    builder.WriteFormat("{0} := {1}",
                        TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                        TranslateValue(chunk, code.SBx, OperandFormat.IMMEDIATE_FLOAT)
                        );
                    break;
                case Operand.LOADK:
                    builder.WriteFormat("{0} := {1}",
                        TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                        TranslateValue(chunk, code.Bx, OperandFormat.CONSTANT)
                        );
                    break;
                case Operand.LOADBOOL:
                    builder.WriteFormat("{0} := {1}",
                        TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                        code.B != 0 ? "true" : "false")
                        .WriteLine(true)
                        .WriteFormat("if ({0}) pc++", code.C);
                    break;
                case Operand.LOADFALSE:
                    builder.WriteFormat("{0} := false",
                        TranslateIsSet(chunk, code.A, OperandFormat.REGISTER));
                    break;
                case Operand.LOADTRUE:
                    builder.WriteFormat("{0} := true",
                        TranslateIsSet(chunk, code.A, OperandFormat.REGISTER));
                    break;
                case Operand.LFALSESKIP:
                    builder.WriteFormat("{0} := false", // pc++
                        TranslateIsSet(chunk, code.A, OperandFormat.REGISTER));
                    chunk.MoveNext();
                    break;
                case Operand.LOADKX:
                    builder.WriteFormat("{0} := {1}",
                        TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                        TranslateValue(chunk, GetNextOperand(chunk).Codepoint, OperandFormat.CONSTANT)
                        );
                    break;
                case Operand.LOADNIL:
                    {
                        var begin = code.A;
                        var count = code.B;
                        for (int i = 0; i <= count; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(TranslateIsSet(chunk, begin + i, OperandFormat.REGISTER));
                        }
                        builder.Write(" := nil");
                    }
                    break;
                case Operand.GETUPVAL:
                    // ? _env[] = UPVALUE
                    builder.WriteFormat("{0} := {1}",
                        TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                        TranslateValue(chunk, code.B, OperandFormat.UPVALUE)
                        ); 
                    break;
                case Operand.SETUPVAL:
                    builder.WriteFormat("{1} := {0}",
                        TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                        TranslateValue(chunk, code.B, OperandFormat.UPVALUE)
                        );
                    break;
                case Operand.GETGLOBAL:
                    Add(TranslateName(chunk, code.A, OperandFormat.REGISTER), 
                        TranslateValue(chunk, code.Bx, OperandFormat.CONSTANT));
                    //builder.WriteFormat("{0} := _G[{1}]",
                    //    TranslateName(chunk, code.A, OperandFormat.REGISTER),
                    //    TranslateValue(chunk, code.Bx, OperandFormat.CONSTANT)
                    //    );
                    break;
                case Operand.SETGLOBAL:
                    builder.WriteFormat("_G[{1}] := {0}",
                        TranslateName(chunk, code.A, OperandFormat.REGISTER),
                        TranslateValue(chunk, code.Bx, OperandFormat.CONSTANT)
                        );
                    break;
                case Operand.SELF:
                    builder.WriteFormat("{0} := {1}",
                        TranslateName(chunk, code.A + 1, OperandFormat.REGISTER),
                        TranslateValue(chunk, code.B, OperandFormat.REGISTER)
                        ).WriteLine(true)
                        .WriteFormat("{0} := {1}[{2}]",
                            TranslateName(chunk, code.A, OperandFormat.REGISTER),
                            TranslateValue(chunk, code.B, OperandFormat.REGISTER),
                            TranslateValue(chunk, code.C, OperandFormat.CONSTANT_STRING)
                        );
                    break;
                case Operand.ADD:
                case Operand.SUB:
                case Operand.MUL:
                case Operand.DIV:
                case Operand.MOD:
                case Operand.POW:
                case Operand.IDIV:
                case Operand.BAND:
                case Operand.BOR:
                case Operand.BXOR:
                case Operand.SHL:
                case Operand.SHR:

                case Operand.EQ:
                case Operand.LE:
                case Operand.LT:
                    builder.WriteFormat("{0} = {1} {2} {3}",
                        TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                        TranslateNotSet(chunk, code.B, OperandFormat.REGISTER), 
                        TranslateOperation(code.Operand),
                        TranslateNotSet(chunk, code.C, OperandFormat.REGISTER));
                    break;
                case Operand.UNM:
                case Operand.NOT:
                case Operand.LEN:
                case Operand.BNOT:
                    builder.WriteFormat("{0} = {1}{2}",
                        TranslateIsSet(chunk, code.A, OperandFormat.REGISTER), 
                        TranslateOperation(code.Operand),
                        TranslateNotSet(chunk, code.B, OperandFormat.REGISTER));
                    break;
                case Operand.CONCAT:
                    {
                        builder.Write(TranslateIsSet(chunk, code.A, OperandFormat.REGISTER))
                        .Write(" = ");
                        var begin = code.A;
                        var count = code.B;
                        for (var i = 0; i < count; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(" .. ");
                            }
                            builder.Write(
                                TranslateValue(chunk, begin + i, OperandFormat.REGISTER));
                        }
                    }
                    break;
                case Operand.JMP:
                    // pc += sj
                    builder.WriteFormat("    goto {0}",
                        TranslateValue(chunk, code.SJ, OperandFormat.JUMP));
                    break;
                case Operand.TEST:
                    builder.WriteFormat("if (not {0} == {1}) then pc++",
                        TranslateNotSet(chunk, code.A, OperandFormat.REGISTER),
                        code.K
                        );
                    break;
                case Operand.TESTSET:
                    builder.WriteFormat("if (not {0} == {1}) then pc++ else {2} := {0} (*)",
                        TranslateNotSet(chunk, code.B, OperandFormat.REGISTER),
                        code.K,
                        TranslateIsSet(chunk, code.A, OperandFormat.REGISTER)
                        );
                    break;
               
                case Operand.CALL:
                    {
                        var fn = TranslateNotSet(chunk, code.A, OperandFormat.REGISTER);
                        var begin = code.A;
                        var count = code.C - 1;
                        if (count > 0)
                        {
                            for (var i = 0; i < count; i++)
                            {
                                if (i > 0)
                                {
                                    builder.Write(", ");
                                }
                                builder.Write(
                                    TranslateIsSet(chunk, begin + i, OperandFormat.REGISTER));
                            }
                            builder.Write(" = ");
                        }
                        builder.Write(fn)
                            .Write('(');
                        begin = code.A + 1;
                        count = code.B - 1;
                        for (var i = 0; i < count; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(TranslateNotSet(chunk, begin + i, OperandFormat.REGISTER));
                        }
                        builder.Write(')');
                    }
                    break;
                case Operand.TAILCALL:
                    {
                        builder.Write("return ")
                                .Write(TranslateNotSet(chunk, code.A, OperandFormat.REGISTER))
                                .Write('(');
                        var begin = code.A + 1;
                        var count = code.B - 1;
                        for (var i = 0; i < count; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(TranslateNotSet(chunk, begin + i, OperandFormat.REGISTER));
                        }
                        builder.Write(')');
                    }
                    break;
                case Operand.RETURN:
                    {
                        builder.Write("return ");
                        var begin = code.A;
                        var count = code.B - 1;
                        for (var i = 0; i < count; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(TranslateNotSet(chunk, begin + i, OperandFormat.REGISTER));
                        }
                    }
                    break;
                case Operand.RETURN0:
                    builder.Write("return");
                    break;
                case Operand.RETURN1:
                    builder.Write("return ")
                        .Write(TranslateNotSet(chunk, code.A, OperandFormat.REGISTER));
                    break;
                case Operand.SETLIST:
                    {
                        var count = code.B == 0 ? code.Codepoint - code.A - 1 : code.B;
                        var offset = 
                            (code.C == 0 ? GetNextOperand(chunk).Codepoint : code.C - 1) % 50;
                        for (int i = 1; i < count; i++)
                        {
                            builder.WriteFormat("{0}[{1}] = {2}",
                                TranslateNotSet(chunk, code.A, OperandFormat.REGISTER),
                                offset,
                                TranslateNotSet(chunk, code.A + i, OperandFormat.REGISTER)
                            );
                        }
                    }
                    break;
                case Operand.SETLISTO:
                    {
                        var count = code.Codepoint - code.A - 1;
                        var offset = code.Bx - code.Bx % 32;
                        for (int i = 1; i < count; i++)
                        {
                            builder.WriteFormat("{0}[{1}] = {2}",
                                TranslateNotSet(chunk, code.A, OperandFormat.REGISTER),
                                offset,
                                TranslateNotSet(chunk, code.A + i, OperandFormat.REGISTER)
                            );
                        }
                    }
                    break;
                case Operand.CLOSE:
                    break;
                case Operand.CLOSURE:
                    {
                        var fn = TranslateValue(chunk, code.A, OperandFormat.REGISTER);
                        if (chunk.MoveNext())
                        {
                            var next = (OperandCode)chunk.CurrentOpcode;
                            Debug.Assert(next.Operand == Operand.SETTABUP);
                            fn = chunk.ConstantItems[code.B].Value.ToString();
                        }
                        var sub = chunk.PrototypeItems[code.Bx];
                        var args = new string[sub.ParameterCount];
                        for (var i = 0; i < args.Length; i++)
                        {
                            args[i] = TranslateName(sub, i, OperandFormat.REGISTER);
                        }
                        // TODO 判断输入参数名
                        builder.WriteFormat("function {0}({1})", fn, string.Join(", ", args)).WriteLine()
                            .WriteIncIndent().WriteFormat(";{0} {1} FNEW {2};",
                                    chunk.CurrentIndex,
                                    chunk.DebugInfo.LineNoItems.Length > chunk.CurrentIndex ?
                                    chunk.DebugInfo.LineNoItems[chunk.CurrentIndex] : 0,
                                    code.A);
                        Decompile(builder, sub);
                        builder.WriteOutdent().Write("end");
                    }
                    break;
                case Operand.VARARG:
                    {
                        var begin = code.A;
                        var count = code.C - 1;
                        for (int i = 0; i < count; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(TranslateIsSet(chunk, begin + i, OperandFormat.REGISTER));
                        }
                        builder.Write(" = ...");
                    }
                    break;
                case Operand.GETTABUP:
                    {
                        var target = TranslateName(chunk, code.B, OperandFormat.UPVALUE);
                        if (target.Equals("_env", StringComparison.OrdinalIgnoreCase))
                        {
                            Add(TranslateName(chunk, code.A, OperandFormat.REGISTER), 
                                TranslateName(chunk, code.C, OperandFormat.CONSTANT_STRING));
                            break;
                        }
                        builder.WriteFormat("{0} := {1}[{2}]",
                            TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                            target,
                            TranslateNotSet(chunk, code.C, OperandFormat.CONSTANT_STRING)
                        );
                    }
                    break;
                case Operand.SETTABUP:
                    builder.WriteFormat("{0}[{1}] := {2}",
                        TranslateNotSet(chunk, code.A, OperandFormat.UPVALUE),
                        TranslateValue(chunk, code.B, OperandFormat.CONSTANT_STRING),
                        TranslateNotSet(chunk, code.C, OperandFormat.REGISTER)
                        );
                    break;
                
                case Operand.GETTABLE:
                    builder.WriteFormat("{0} := {1}[{2}]",
                        TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                        TranslateNotSet(chunk, code.B, OperandFormat.REGISTER),
                        TranslateNotSet(chunk, code.C, OperandFormat.REGISTER)
                        );
                    break;
       
                case Operand.SETTABLE:
                    builder.WriteFormat("{0}[{1}] := {2}",
                        TranslateNotSet(chunk, code.A, OperandFormat.REGISTER),
                        TranslateNotSet(chunk, code.B, OperandFormat.REGISTER),
                        TranslateNotSet(chunk, code.C, OperandFormat.REGISTER_K)
                        );
                    break;

                case Operand.NEWTABLE:
                    var arraySize = code.C;
                    if (code.K)
                    {
                        arraySize += GetNextOperand(chunk).Ax * (code.Extractor.C.Max + 1);
                    }
                    builder.WriteFormat("{0} := new table(array: {1}, dict: {2})", 
                        TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                        arraySize,
                        code.B == 0 ? 0 : (1 << (code.B - 1)));
                    break;
                case Operand.GETI:
                    builder.WriteFormat("{0} := {1}[{2}]",
                        TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                        TranslateNotSet(chunk, code.B, OperandFormat.REGISTER),
                        code.C);
                    break;
                case Operand.GETFIELD:
                    builder.WriteFormat("{0} := {1}[{2}]",
                        TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                        TranslateNotSet(chunk, code.B, OperandFormat.REGISTER),
                        TranslateValue(chunk, code.C, OperandFormat.CONSTANT_STRING));
                    break;
                case Operand.SETI:
                    builder.WriteFormat("{0}[{1}] := {2}",
                         TranslateNotSet(chunk, code.A, OperandFormat.REGISTER),
                         code.B,
                         TranslateNotSet(chunk, code.C, OperandFormat.REGISTER_K));
                    break;
                case Operand.SETFIELD:
                    builder.WriteFormat("{0}[{1}] := {2}",
                         TranslateNotSet(chunk, code.A, OperandFormat.REGISTER),
                         TranslateValue(chunk, code.B, OperandFormat.CONSTANT_STRING),
                         TranslateNotSet(chunk, code.C, OperandFormat.REGISTER_K));
                    break;
                case Operand.ADDI:
                case Operand.SHRI:
                    {
                        builder.WriteFormat("{0} := {1} {2} {3}",
                            TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                            TranslateNotSet(chunk, code.B, OperandFormat.REGISTER),
                            TranslateOperation(code.Operand),
                            code.SC);
                    }
                    break;
                case Operand.SHLI:
                    {
                        builder.WriteFormat("{0} := {3} {2} {1}",
                            TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                            TranslateNotSet(chunk, code.B, OperandFormat.REGISTER),
                            TranslateOperation(code.Operand),
                            code.SC);
                    }
                    break;
                case Operand.ADDK:
                case Operand.SUBK:
                case Operand.MULK:
                case Operand.MODK:
                case Operand.POWK:
                case Operand.DIVK:
                case Operand.IDIVK:
                case Operand.BANDK:
                case Operand.BORK:
                case Operand.BXORK:
                    {
                        builder.WriteFormat("{0} = {1} {2} {3}",
                            TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                            TranslateNotSet(chunk, code.B, OperandFormat.REGISTER),
                            TranslateOperation(code.Operand),
                            TranslateNotSet(chunk, code.C, OperandFormat.REGISTER_K));
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
                case Operand.EQK:
                    builder.WriteFormat("if (({0} == {1}) ~= {2}) then pc++",
                        TranslateNotSet(chunk, code.A, OperandFormat.REGISTER),
                        TranslateNotSet(chunk, code.B, OperandFormat.REGISTER_K),
                        code.K);
                    break;
                case Operand.EQI:
                case Operand.LTI:
                case Operand.LEI:
                case Operand.GTI:
                case Operand.GEI:
                    builder.WriteFormat("if (({0} {1} {2}) ~= {3}) then pc++",
                            TranslateNotSet(chunk, code.A, OperandFormat.REGISTER),
                            TranslateOperation(code.Operand),
                            code.SB,
                            code.K);
                    break;
                case Operand.VARARGPREP:
                    break;
                case Operand.FORLOOP:
                    // update counters; if loop continues then pc-=Bx;
                    break;
                case Operand.FORPREP:
                    //<check values and prepare counters>; if not to run then pc+= Bx + 1;
                    break;
                case Operand.TFORLOOP:
                    builder.WriteFormat("if {0} ~= nil then {{ {1}={0}; pc -= {2} }}",
                        TranslateNotSet(chunk, code.A + 2, OperandFormat.REGISTER),
                        TranslateNotSet(chunk, code.A, OperandFormat.REGISTER),
                        code.Bx
                        );
                    break;
                case Operand.TFORPREP:
                    // create upvalue for R[A + 3]; pc+=Bx
                    break;
                case Operand.TFORCALL:
                    {
                        var begin = code.A + 4;
                        var count = code.C;
                        for (var i = 0; i < count; i++)
                        {
                            if (i > 0)
                            {
                                builder.Write(", ");
                            }
                            builder.Write(TranslateIsSet(chunk, begin + 1, OperandFormat.REGISTER));
                        }
                        builder.WriteFormat(" := {0}({1}, {2})",
                            TranslateNotSet(chunk, code.A, OperandFormat.REGISTER),
                            TranslateNotSet(chunk, code.A + 1, OperandFormat.REGISTER),
                            TranslateNotSet(chunk, code.A + 2, OperandFormat.REGISTER)
                            );
                    }
                    break;
                case Operand.EXTRAARG:
                    break;
            }
        }

        private string TranslateIsSet(LuaChunk chunk, int value, OperandFormat format)
        {
            var res = TranslateValue(chunk, value, format);
            Remove(res);
            if (format == OperandFormat.FUNCTION && TryGetLocal(chunk, value, out var fn))
            {
                return fn;
            }
            return res;
        }


        private string TranslateNotSet(LuaChunk chunk, int value, OperandFormat format)
        {
            var res = TranslateValue(chunk, value, format);
            if (format is OperandFormat.REGISTER or OperandFormat.REGISTER_K)
            {
                if (TryGet(res, out var fn))
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


        private static string TranslateConstant(LuaConstant constant)
        {
            return constant.Type switch
            {
                LuaConstantType.Null => "nil",
                LuaConstantType.Bool => (bool)constant.Value == true ? "true" : "false",
                LuaConstantType.Number => constant.Value.ToString() ?? string.Empty,
                LuaConstantType.String => $"\"{constant.Value}\"",
                _ => throw new NotImplementedException(),
            };
        }

        private static string? GetLocalName(LuaChunk chunk, int index)
        {
            if (chunk.DebugInfo.LocalItems.Length > index)
            {
                return chunk.DebugInfo.LocalItems[index].Name;
            }
            return null;
        }
        private static bool TryGetLocal(LuaChunk chunk, int index, [NotNullWhen(true)] out string? varName)
        {
            if (chunk.DebugInfo.LocalItems.Length > index)
            {
                var item = chunk.DebugInfo.LocalItems[index];
                var current = 0UL;
                current = chunk.CurrentIndex >= chunk.DebugInfo.AbsoluteLineItems.Length ? 0 : chunk.DebugInfo.AbsoluteLineItems[Math.Max(chunk.CurrentIndex, 0)].ProgramCounter;
                //for (int i = 0; i <= chunk.CurrentIndex; i++)
                //{
                //    current += chunk.DebugInfo.AbsoluteLineItems[i].ProgramCounter;
                //}
                if (item.StartPc < current && item.EndPc > current)
                {
                    varName = chunk.DebugInfo.LocalItems[index].Name;
                    return true;
                }
            }
            varName = null;
            return false;
        }
        private static string TranslateOperation(Operand op)
        {
            return op switch
            {
                Operand.ADD or Operand.ADDK or Operand.ADDI => "+",
                Operand.SUB or Operand.SUBK => "-",
                Operand.MUL or Operand.MULK => "*",
                Operand.DIV or Operand.DIVK => "/",
                Operand.MOD or Operand.MODK => "%",
                Operand.POW or Operand.POWK => "^",
                Operand.IDIV or Operand.IDIVK => "//",
                Operand.BAND or Operand.BANDK => "&",
                Operand.BOR or Operand.BORK => "|",
                Operand.BXOR or Operand.BXORK => "~",
                Operand.SHL => "<<",
                Operand.SHR => ">>",
                Operand.CONCAT => "..",

                Operand.UNM => "-",
                Operand.NOT => "not ",
                Operand.LEN => "#",
                Operand.BNOT => "~",

                Operand.EQ or Operand.EQK or Operand.EQI => "==",
                Operand.LE or Operand.LEI => "<=",
                Operand.LT or Operand.LTI => "<",
                Operand.GTI => ">",
                Operand.GEI => ">=",
                _ => throw new NotImplementedException(),
            };
        }
        private static string TranslateOperation(int val)
        {
            return val switch
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
                _ => throw new System.NotImplementedException(),
            };
        }


        
        private static OperandCode? GetNextOperand(LuaChunk chunk)
        {
            if (chunk.MoveNext())
            {
                return (OperandCode)chunk.CurrentOpcode;
            }
            return null;
        }

        private static string TranslateValue(LuaChunk chunk, int val, OperandFormat format)
        {
            return format switch
            {
                OperandFormat.RAW or OperandFormat.IMMEDIATE_INTEGER => val.ToString() ?? string.Empty,
                OperandFormat.IMMEDIATE_FLOAT => BitConverter.ToSingle(BitConverter.GetBytes(val)).ToString(),
                OperandFormat.REGISTER or OperandFormat.REGISTER_K => $"r{val}",
                OperandFormat.UPVALUE => chunk.DebugInfo.UpValueNameItems.Length > val ? chunk.DebugInfo.UpValueNameItems[val] : $"u{val}",
                OperandFormat.CONSTANT or OperandFormat.CONSTANT_INTEGER 
                or OperandFormat.CONSTANT_STRING => TranslateConstant(chunk.ConstantItems[val]),
                OperandFormat.FUNCTION => $"f{val}",
                OperandFormat.JUMP_NEGATIVE => (-val).ToString(),
                OperandFormat.JUMP => val.ToString(),
                _ => throw new NotImplementedException(),
            };
        }

        private static string TranslateName(LuaChunk chunk, int val, OperandFormat format)
        {
            if (format == OperandFormat.CONSTANT_STRING)
            {
                return chunk.ConstantItems[val].Value.ToString() ?? string.Empty;
            }
            return TranslateValue(chunk, val, format);
        }
    }
}
