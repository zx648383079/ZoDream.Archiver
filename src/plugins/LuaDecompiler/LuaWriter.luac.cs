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
                        if (Version is LuaVersion.Lua51 or LuaVersion.Lua50)
                        {
                            count = code.B - code.A + 1;
                        }
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
                    {
                        var c = TranslateValue(chunk, code.C, 
                            Version is LuaVersion.Lua54Beta ? OperandFormat.CONSTANT_STRING : OperandFormat.REGISTER_K);
                        builder.WriteFormat("{0} := {1}",
                            TranslateName(chunk, code.A + 1, OperandFormat.REGISTER),
                            TranslateValue(chunk, code.B, OperandFormat.REGISTER)
                            ).WriteLine(true)
                            .WriteFormat("{0} := {1}[{2}]",
                                TranslateName(chunk, code.A, OperandFormat.REGISTER),
                                TranslateValue(chunk, code.B, OperandFormat.REGISTER),
                                c
                            );
                    }
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
                    {
                        var bcType = Version is LuaVersion.Lua53 or LuaVersion.Lua52 or LuaVersion.Lua51 or LuaVersion.Lua50 ?
                            OperandFormat.REGISTER_K : OperandFormat.REGISTER;
                        var b = TranslateNotSet(chunk, code.B, 
                            bcType);
                        var c = TranslateNotSet(chunk, code.C,
                            bcType);
                        builder.WriteFormat("{0} = {1} {2} {3}",
                            TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                            b,
                            TranslateOperation(code.Operand),
                            c);
                    }
                    break;
                case Operand.UNM:
                case Operand.NOT:
                case Operand.LEN:
                case Operand.BNOT:
                    {
                        var b = TranslateNotSet(chunk, code.B, OperandFormat.REGISTER);
                        builder.WriteFormat("{0} = {1}{2}",
                            TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                            TranslateOperation(code.Operand),
                            b);
                    }
                    break;
                case Operand.CONCAT:
                    {
                        var begin = code.A;
                        var count = code.B;
                        var args = new string[count];
                        for (var i = 0; i < count; i++)
                        {
                            args[i] = TranslateNotSet(chunk, begin + i, OperandFormat.REGISTER);
                        }
                        builder.WriteFormat("{0} = {1}",
                            TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                            string.Join(" .. ", args));
                        
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
                        var begin = code.A + 1;
                        var count = code.B - 1;
                        var args = new string[count];
                        for (var i = 0; i < count; i++)
                        {
                            args[i] = TranslateNotSet(chunk, begin + i, OperandFormat.REGISTER);
                        }

                        begin = code.A;
                        count = code.C - 1;
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
                        builder.WriteFormat("{0}({1})", fn, string.Join(", ", args));
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
                        if (TryMoveNext(chunk, Operand.RETURN, out var next) && next.B <= 1)
                        {
                            chunk.MoveNext();
                        }
                        TryMoveNext(chunk, Operand.RETURN0);
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
                    TryMoveNext(chunk, Operand.RETURN0);
                    break;
                case Operand.SETLIST:
                    {
                        var begin = 1;
                        var count = code.B;
                        var lt = code.C;
                        var rt = code.A;
                        if (Version is LuaVersion.Lua54Beta)
                        {
                            count = code.VB;
                            lt = code.VC;
                        }
                        if (Version is LuaVersion.Lua50)
                        {
                            count = code.Bx % 32 + 1;
                            lt = code.Bx - code.Bx % 32;
                        }
                        if (Version is LuaVersion.Lua53 or LuaVersion.Lua52 or LuaVersion.Lua51)
                        {
                            lt = (code.C - 1) * 50;
                        }
                        for (int i = 0; i < count; i++)
                        {
                            if (i > 0)
                            {
                                builder.WriteLine(true);
                            }
                            builder.WriteFormat("{0}[{1}] = {2}",
                                TranslateNotSet(chunk, code.A, OperandFormat.REGISTER),
                                lt + begin + i,
                                TranslateNotSet(chunk, rt + begin + i, OperandFormat.REGISTER)
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
                        RemoveTemporary();
                        var fn = TranslateValue(chunk, code.A, OperandFormat.REGISTER);
                        if (Version > LuaVersion.Lua53 && chunk.MoveNext())
                        {
                            var next = (OperandCode)chunk.CurrentOpcode;
                            Debug.Assert(next.Operand == Operand.SETTABUP);
                            Debug.Assert(next.C == code.A);
                            fn = TranslateName(chunk, next.B, OperandFormat.CONSTANT_STRING);
                        }
                        var sub = chunk.PrototypeItems[code.Bx];
                        var args = new string[sub.ParameterCount];
                        var begin = 0;
                        if (Version is LuaVersion.Lua51 or LuaVersion.Lua50)
                        {
                            // sub = chunk
                            begin = code.A;
                        }
                        for (var i = 0; i < args.Length; i++)
                        {
                            args[i] = TranslateNotSet(sub, begin + i, OperandFormat.REGISTER);
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
                        if (Version is LuaVersion.Lua53 or LuaVersion.Lua52)
                        {
                            count = code.B - 1;
                        } else if (Version is LuaVersion.Lua51 or LuaVersion.Lua50)
                        {
                            count = code.B;
                        }
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
                        var cType = Version is LuaVersion.Lua53 or LuaVersion.Lua52 ?
                            OperandFormat.REGISTER_K : OperandFormat.CONSTANT_STRING;
                        if (target.Equals("_env", StringComparison.OrdinalIgnoreCase))
                        {
                            Add(TranslateName(chunk, code.A, OperandFormat.REGISTER), 
                                TranslateName(chunk, code.C, cType));
                            break;
                        }
                        var c = TranslateNotSet(chunk, code.C, cType);
                        builder.WriteFormat("{0} := {1}[{2}]",
                            TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                            target,
                            c
                        );
                    }
                    break;
                case Operand.SETTABUP:
                    builder.WriteFormat("{0}[{1}] := {2}",
                        TranslateNotSet(chunk, code.A, OperandFormat.UPVALUE),
                        TranslateValue(chunk, code.B, Version is LuaVersion.Lua53 or LuaVersion.Lua52 ?
                            OperandFormat.REGISTER_K : OperandFormat.CONSTANT_STRING),
                        TranslateNotSet(chunk, code.C, OperandFormat.REGISTER)
                        );
                    break;
                
                case Operand.GETTABLE:
                    {
                        var b = TranslateNotSet(chunk, code.B, OperandFormat.REGISTER);
                        var c = TranslateNotSet(chunk, code.C, OperandFormat.REGISTER);
                        builder.WriteFormat("{0} := {1}[{2}]",
                            TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                            b,
                            c
                            );
                    }
                    break;
       
                case Operand.SETTABLE:
                    builder.WriteFormat("{0}[{1}] := {2}",
                        TranslateNotSet(chunk, code.A, OperandFormat.REGISTER),
                        TranslateNotSet(chunk, code.B, 
                            Version is LuaVersion.Lua53 or LuaVersion.Lua52 ? 
                            OperandFormat.REGISTER_K : OperandFormat.REGISTER),
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
                    {
                        var b = TranslateNotSet(chunk, code.B, OperandFormat.REGISTER);
                        builder.WriteFormat("{0} := {1}[{2}]",
                            TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                            b,
                            code.C);
                    }
                    break;
                case Operand.GETFIELD:
                    {
                        var b = TranslateNotSet(chunk, code.B, OperandFormat.REGISTER);
                        builder.WriteFormat("{0} := {1}[{2}]",
                            TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                            b,
                            TranslateValue(chunk, code.C, OperandFormat.CONSTANT_STRING));
                    }
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
                        var b = TranslateNotSet(chunk, code.B, OperandFormat.REGISTER);
                        builder.WriteFormat("{0} := {3} {2} {1}",
                            TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                            b,
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
                        var b = TranslateNotSet(chunk, code.B, OperandFormat.REGISTER);
                        var c = TranslateNotSet(chunk, code.C, OperandFormat.REGISTER_K);
                        builder.WriteFormat("{0} = {1} {2} {3}",
                            TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                            b,
                            TranslateOperation(code.Operand),
                            c);
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
                    {
                        var b = TranslateNotSet(chunk, code.A + 2, OperandFormat.REGISTER);
                        var c = TranslateNotSet(chunk, code.A + 1, OperandFormat.REGISTER);
                        var a = TranslateIsSet(chunk, code.A, OperandFormat.REGISTER);
                        builder.WriteFormat("{0} += {1}",
                            TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                            b
                            ).WriteLine(true);
                        if (Version is LuaVersion.Lua50)
                        {
                            builder.WriteFormat("if {0} <?= {1} then pc+= {2}",
                                a,
                                c,
                                code.SBx
                                );
                        } else
                        {
                            builder.WriteFormat("if {0} <?= {1} then {{ pc+= {2}; {3} = {0} }}",
                                a,
                                c,
                                code.SBx,
                                TranslateIsSet(chunk, code.A + 3, OperandFormat.REGISTER)
                                );
                        }
                    }
                    break;
                case Operand.FORPREP:
                    //<check values and prepare counters>; if not to run then pc+= Bx + 1;
                    {
                        var b = TranslateNotSet(chunk, code.A + 2, OperandFormat.REGISTER);
                        builder.WriteFormat("{0} -= {1}; pc += {2}",
                            TranslateIsSet(chunk, code.A, OperandFormat.REGISTER),
                            b, 
                            code.SBx
                            );
                    }
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
                            builder.Write(TranslateIsSet(chunk, begin + i, OperandFormat.REGISTER));
                        }
                        builder.WriteFormat(" := {0}({1}, {2})",
                            TranslateNotSet(chunk, code.A, OperandFormat.REGISTER),
                            TranslateNotSet(chunk, code.A + 1, OperandFormat.REGISTER),
                            TranslateNotSet(chunk, code.A + 2, OperandFormat.REGISTER)
                            );
                    }
                    break;
                case Operand.TFORLOOP:
                    {
                        if (Version is LuaVersion.Lua51 or LuaVersion.Lua50)
                        {
                            var begin = code.A + 3;
                            var count = code.C;
                            if (Version is LuaVersion.Lua50)
                            {
                                begin = code.A + 2;
                                count = code.C + 1;
                            }
                            for (var i = 0; i < count; i++)
                            {
                                if (i > 0)
                                {
                                    builder.Write(", ");
                                }
                                builder.Write(TranslateIsSet(chunk, begin + i, OperandFormat.REGISTER));
                            }
                            builder.WriteFormat(" := {0}({1}, {2})",
                                TranslateNotSet(chunk, code.A, OperandFormat.REGISTER),
                                TranslateNotSet(chunk, code.A + 1, OperandFormat.REGISTER),
                                TranslateNotSet(chunk, code.A + 2, OperandFormat.REGISTER)
                                );
                            if (Version is LuaVersion.Lua50)
                            {
                                builder.WriteFormat("if {0} ~= nil then pc++",
                                    TranslateNotSet(chunk, code.A + 2, OperandFormat.REGISTER)
                                );
                            } else
                            {
                                builder.WriteFormat("if {0} ~= nil then {1} = {0} else pc++",
                                    TranslateNotSet(chunk, code.A + 3, OperandFormat.REGISTER),
                                    TranslateNotSet(chunk, code.A + 2, OperandFormat.REGISTER)
                                );
                            }
                        } else
                        {
                            var a = TranslateNotSet(chunk, code.A +
                            (Version is LuaVersion.Lua53 or LuaVersion.Lua52 ? 1 : 2),
                            OperandFormat.REGISTER);
                            builder.WriteFormat("if {0} ~= nil then {{ {1}={0}; pc += {2} }}",
                                a,
                                TranslateNotSet(chunk, code.A, OperandFormat.REGISTER),
                                Version is LuaVersion.Lua53 or LuaVersion.Lua52
                                ? code.SBx : (-code.Bx)
                            );
                        }
                    }
                    break;
                case Operand.EXTRAARG:
                    break;
            }
        }
        /// <summary>
        /// 查看下一个是否是，不移动指针
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="op"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        private static bool TryMoveNext(LuaChunk chunk, Operand op, [NotNullWhen(true)] out OperandCode? next)
        {
            if (chunk.CanMoveNext && chunk.OpcodeItems[chunk.CurrentIndex + 1] is OperandCode res && res.Operand == op)
            {
                next = res;
                return true;
            }
            next = null;
            return false;
        }

        /// <summary>
        /// 查看下一个是否是，是则移动指针
        /// </summary>
        /// <param name="chunk"></param>
        /// <param name="op"></param>
        private static void TryMoveNext(LuaChunk chunk, Operand op)
        {
            if (TryMoveNext(chunk, op, out _)) 
            {
                chunk.MoveNext();
            }
        }

        private string TranslateIsSet(LuaChunk chunk, int value, OperandFormat format)
        {
            var res = TranslateValue(chunk, value, format);
            Remove(res);
            if (format == OperandFormat.REGISTER && TryGetLocal(chunk, value, out var fn))
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

        private bool TryGetLocal(LuaChunk chunk, int index, [NotNullWhen(true)] out string? varName)
        {
            if (chunk.DebugInfo.LocalItems.Length > index)
            {
                var item = chunk.DebugInfo.LocalItems[index];
                var pc = Math.Max(chunk.CurrentIndex, 0);
                var current = 0UL;
                if (Version is LuaVersion.LuaJit21 or LuaVersion.LuaJit2 or LuaVersion.LuaJit1)
                {
                    current = chunk.DebugInfo.AbsoluteLineItems[pc].ProgramCounter;
                    //for (int i = 0; i <= chunk.CurrentIndex; i++)
                    //{
                    //    current += chunk.DebugInfo.AbsoluteLineItems[i].ProgramCounter;
                    //}
                }
                else
                {
                    current = (ulong)pc + 2;
                }
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
            if (format == OperandFormat.REGISTER_K)
            {
                var extractor = (chunk.OpcodeItems[0] as OperandCode).Extractor;
                if (extractor.IsK(val))
                {
                    return chunk.ConstantItems[extractor.DecodeK(val)].Value.ToString();
                }
            }
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
