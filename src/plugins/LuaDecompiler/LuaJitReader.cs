﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Net;
using System.Reflection.Emit;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Language;
using ZoDream.Shared.Language.AST;

namespace ZoDream.LuaDecompiler
{
    public partial class LuaJitReader : ILanguageReader
    {
        public GlobalExpression Read(Stream input)
        {
            var data = ReadBytecode(input);
            return CreateBuilder(data);
        }

        private GlobalExpression CreateBuilder(LuaBytecode data)
        {
            var builder = new GlobalExpression();
            foreach (var item in Translate(data.MainChunk))
            {
                builder.Add(item);
            }
            return builder;
        }

        private IEnumerable<Expression> Translate(LuaChunk chunk)
        {
            var index = 0;
            foreach (var item in chunk.OpcodeItems)
            {
                yield return Translate(chunk, ++index, (JitOperandCode)item);
            }
        }

        private Expression Translate(LuaChunk chunk,
            int index, JitOperandCode code)
        {
            var attr = JitOperandExtractor.GetAttribute(code.Operand);
            if (attr is null)
            {
                return Expression.Constant(null);
            }
            switch (code.Operand)
            {
                case JitOperand.ISLT:
                    return Expression.LessThan(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType));
                case JitOperand.ISGE:
                    return Expression.GreaterThanOrEqual(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType));
                case JitOperand.ISLE:
                    return Expression.LessThanOrEqual(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType));
                case JitOperand.ISGT:
                    return Expression.GreaterThan(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType));
                case JitOperand.ISEQV:
                    return Expression.Equal(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType));
                case JitOperand.ISNEV:
                    return Expression.NotEqual(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType));
                case JitOperand.ISEQS:
                    return Expression.Equal(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType));
                case JitOperand.ISNES:
                    return Expression.NotEqual(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType));
                case JitOperand.ISEQN:
                    return Expression.Equal(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType));
                case JitOperand.ISNEN:
                    return Expression.NotEqual(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType));
                case JitOperand.ISEQP:
                    return Expression.Equal(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType));
                case JitOperand.ISNEP:
                    return Expression.NotEqual(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType));
                case JitOperand.ISTC:
                    {
                        var d = Translate(chunk, index, (int)code.D, attr.CDType);
                        return Expression.IfThen(d,
                            Expression.Assign(
                                Translate(chunk, index, code.A, attr.AType),
                                d));
                    }
                case JitOperand.ISFC:
                    {
                        var d = Translate(chunk, index, (int)code.D, attr.CDType);
                        return Expression.IfThen(Expression.Not(d),
                            Expression.Assign(
                                Translate(chunk, index, code.A, attr.AType),
                                d));
                    }
                case JitOperand.IST:
                    return Expression.IsTrue(Translate(chunk, index, (int)code.D, attr.CDType));
                case JitOperand.ISF:
                    return Expression.IsFalse(Translate(chunk, index, (int)code.D, attr.CDType));
                case JitOperand.ISTYPE:
                    return Expression.TypeIs(Translate(chunk, index, (int)code.A, attr.AType), typeof(GlobalExpression));
                case JitOperand.ISNUM:
                    return Expression.TypeIs(Translate(chunk, index, (int)code.A, attr.AType), typeof(Int32));
                case JitOperand.MOV:
                    return Expression.Assign(
                                Translate(chunk, index, code.A, attr.AType),
                                Translate(chunk, index, (int)code.D, attr.CDType));
                case JitOperand.NOT:
                    return Expression.Assign(
                                Translate(chunk, index, code.A, attr.AType),
                                Expression.Not(Translate(chunk, index, (int)code.D, attr.CDType)));
                case JitOperand.UNM:
                    return Expression.Assign(
                                Translate(chunk, index, code.A, attr.AType),
                                Expression.Negate(Translate(chunk, index, (int)code.D, attr.CDType)));
                case JitOperand.LEN:
                    return Expression.Assign(
                                Translate(chunk, index, code.A, attr.AType),
                                GlobalExpression.SizeOf(Translate(chunk, index, (int)code.D, attr.CDType)));
                case JitOperand.ADDVN:
                    return Expression.Assign(
                                Translate(chunk, index, code.A, attr.AType),
                                Expression.Add(Translate(chunk, index, code.B, attr.BType),
                                Translate(chunk, index, code.C, attr.CDType))
                                );
                case JitOperand.SUBVN:
                    return Expression.Assign(
                         Translate(chunk, index, code.A, attr.AType),
                         Expression.Subtract(Translate(chunk, index, code.B, attr.BType),
                         Translate(chunk, index, code.C, attr.CDType))
                         );
                case JitOperand.MULVN:
                    return Expression.Assign(
                         Translate(chunk, index, code.A, attr.AType),
                         Expression.Multiply(Translate(chunk, index, code.B, attr.BType),
                         Translate(chunk, index, code.C, attr.CDType))
                         );
                case JitOperand.DIVVN:
                    return Expression.Assign(
                         Translate(chunk, index, code.A, attr.AType),
                         Expression.Divide(Translate(chunk, index, code.B, attr.BType),
                         Translate(chunk, index, code.C, attr.CDType))
                         );
                case JitOperand.MODVN:
                    return Expression.Assign(
                         Translate(chunk, index, code.A, attr.AType),
                         Expression.Modulo(Translate(chunk, index, code.B, attr.BType),
                         Translate(chunk, index, code.C, attr.CDType))
                         );
                case JitOperand.ADDNV:
                    return Expression.Assign(
                                Translate(chunk, index, code.A, attr.AType),
                                Expression.Add(
                                    Translate(chunk, index, code.C, attr.CDType),
                                    Translate(chunk, index, code.B, attr.BType)
                                    )
                                );
                case JitOperand.SUBNV:
                    return Expression.Assign(
                               Translate(chunk, index, code.A, attr.AType),
                               Expression.Subtract(
                                   Translate(chunk, index, code.C, attr.CDType),
                                   Translate(chunk, index, code.B, attr.BType)
                                   )
                               );
                case JitOperand.MULNV:
                    return Expression.Assign(
                               Translate(chunk, index, code.A, attr.AType),
                               Expression.Multiply(
                                   Translate(chunk, index, code.C, attr.CDType),
                                   Translate(chunk, index, code.B, attr.BType)
                                   )
                               );
                case JitOperand.DIVNV:
                    return Expression.Assign(
                               Translate(chunk, index, code.A, attr.AType),
                               Expression.Divide(
                                   Translate(chunk, index, code.C, attr.CDType),
                                   Translate(chunk, index, code.B, attr.BType)
                                   )
                               );
                case JitOperand.MODNV:
                    return Expression.Assign(
                               Translate(chunk, index, code.A, attr.AType),
                               Expression.Modulo(
                                   Translate(chunk, index, code.C, attr.CDType),
                                   Translate(chunk, index, code.B, attr.BType)
                                   )
                               );
                case JitOperand.ADDVV:
                    return Expression.Assign(
                                Translate(chunk, index, code.A, attr.AType),
                                Expression.Add(Translate(chunk, index, code.B, attr.BType),
                                Translate(chunk, index, code.C, attr.CDType))
                                );
                case JitOperand.SUBVV:
                    return Expression.Assign(
                                Translate(chunk, index, code.A, attr.AType),
                                Expression.Subtract(Translate(chunk, index, code.B, attr.BType),
                                Translate(chunk, index, code.C, attr.CDType))
                                );
                case JitOperand.MULVV:
                    return Expression.Assign(
                                Translate(chunk, index, code.A, attr.AType),
                                Expression.Multiply(Translate(chunk, index, code.B, attr.BType),
                                Translate(chunk, index, code.C, attr.CDType))
                                );
                case JitOperand.DIVVV:
                    return Expression.Assign(
                                Translate(chunk, index, code.A, attr.AType),
                                Expression.Divide(Translate(chunk, index, code.B, attr.BType),
                                Translate(chunk, index, code.C, attr.CDType))
                                );
                case JitOperand.MODVV:
                    return Expression.Assign(
                                Translate(chunk, index, code.A, attr.AType),
                                Expression.Modulo(Translate(chunk, index, code.B, attr.BType),
                                Translate(chunk, index, code.C, attr.CDType))
                                );
                case JitOperand.POW:
                    return Expression.Assign(
                                Translate(chunk, index, code.A, attr.AType),
                                Expression.Power(Translate(chunk, index, code.B, attr.BType),
                                Translate(chunk, index, code.C, attr.CDType))
                                );
                case JitOperand.CAT:
                    return Expression.Assign(
                        Translate(chunk, index, code.A, attr.AType),
                        Expression.Add(Translate(chunk, index, code.B, attr.BType),
                                Translate(chunk, index, code.C, attr.CDType))
                        );
                case JitOperand.KSTR:
                    return Expression.Assign(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType)
                        );
                case JitOperand.KCDATA:
                    return Expression.Assign(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType)
                        );
                case JitOperand.KSHORT:
                    return Expression.Assign(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType)
                        );
                case JitOperand.KNUM:
                    return Expression.Assign(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType)
                        );
                case JitOperand.KPRI:
                    return Expression.Assign(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType)
                        );
                case JitOperand.KNIL:
                    {
                        var items = new List<Expression>();
                        for (var i = code.A; i <= code.D; i++)
                        {
                            items.Add(
                                Translate(chunk, index, i, JitOperandFormat.VAR)
                             );
                        }
                        return GlobalExpression.Assign(items, Expression.Constant(null));
                    }
                case JitOperand.UGET:
                    return Expression.Assign(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType)
                        );
                case JitOperand.USETV:
                    return Expression.Assign(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType)
                        );
                case JitOperand.USETS:
                    return Expression.Assign(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType)
                        );
                case JitOperand.USETN:
                    return Expression.Assign(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType)
                        );
                case JitOperand.USETP:
                    return Expression.Assign(
                        Translate(chunk, index, code.A, attr.AType),
                        Translate(chunk, index, (int)code.D, attr.CDType)
                        );
                case JitOperand.UCLO:
                    break;
                case JitOperand.FNEW:
                    return GlobalExpression.Function(TranslateName(chunk, index, code.A, attr.AType), Translate(chunk, index, (int)code.D, attr.CDType));
                case JitOperand.TNEW:
                    return Expression.Assign(
                        Translate(chunk, index, code.A, attr.AType),
                        GlobalExpression.NewMap(
                            code.D & 0b0000011111111111,
                            (uint)Math.Pow(2, code.D >> 11)
                            )
                        );
                case JitOperand.TDUP:
                    return Expression.Assign(
                        Translate(chunk, index, code.A, attr.AType),
                        GlobalExpression.Clone(Translate(chunk, index, (int)code.D, attr.CDType))
                        );
                case JitOperand.GGET:
                    return Expression.Assign(
                        Translate(chunk, index, code.A, attr.AType),
                        Expression.ArrayIndex(
                            Expression.Constant("_env"),
                            Translate(chunk, index, (int)code.D, attr.CDType))
                        );
                case JitOperand.GSET:
                    return Expression.Assign(
                        Expression.ArrayIndex(
                            Expression.Constant("_env"),
                            Translate(chunk, index, (int)code.D, attr.CDType)),
                        Translate(chunk, index, code.A, attr.AType)
                        );
                case JitOperand.TGETV:
                    return Expression.Assign(
                        Translate(chunk, index, code.A, attr.AType),
                        Expression.ArrayIndex(
                            Translate(chunk, index, code.B, attr.BType),
                            Translate(chunk, index, code.C, attr.CDType))
                        );
                case JitOperand.TGETS:
                    return Expression.Assign(
                        Translate(chunk, index, code.A, attr.AType),
                        GlobalExpression.Field(
                            Translate(chunk, index, code.B, attr.BType),
                            Translate(chunk, index, code.C, attr.CDType))
                        );
                case JitOperand.TGETB:
                    return Expression.Assign(
                        Translate(chunk, index, code.A, attr.AType),
                        Expression.ArrayIndex(
                            Translate(chunk, index, code.B, attr.BType),
                            Translate(chunk, index, code.C, attr.CDType))
                        );
                case JitOperand.TGETR:
                    return Expression.Assign(
                        Translate(chunk, index, code.A, attr.AType),
                        Expression.ArrayIndex(
                            Translate(chunk, index, code.B, attr.BType),
                            Translate(chunk, index, code.C, attr.CDType))
                        );
                case JitOperand.TSETV:
                    return Expression.Assign(
                        Expression.ArrayIndex(
                            Translate(chunk, index, code.B, attr.BType),
                            Translate(chunk, index, code.C, attr.CDType)),
                        Translate(chunk, index, code.A, attr.AType)
                        );
                case JitOperand.TSETS:
                    return Expression.Assign(
                         GlobalExpression.Field(
                             Translate(chunk, index, code.B, attr.BType),
                             Translate(chunk, index, code.C, attr.CDType)),
                         Translate(chunk, index, code.A, attr.AType)
                         );
                case JitOperand.TSETB:
                    return Expression.Assign(
                        Expression.ArrayIndex(
                            Translate(chunk, index, code.B, attr.BType),
                            Translate(chunk, index, code.C, attr.CDType)),
                        Translate(chunk, index, code.A, attr.AType)
                        );
                case JitOperand.TSETM:
                    break;
                case JitOperand.TSETR:
                    return Expression.Assign(
                         Expression.ArrayIndex(
                             Translate(chunk, index, code.B, attr.BType),
                             Translate(chunk, index, code.C, attr.CDType)),
                         Translate(chunk, index, code.A, attr.AType)
                         );
                case JitOperand.CALLM:
                    {
                        var items = new Expression[code.D];
                        for (int i = 0; i < items.Length; i++)
                        {
                            items[i] = Translate(chunk, index, code.A + i, JitOperandFormat.VAR);
                        }
                        var args = new Expression[code.B - 1];
                        for (int i = 0; i < args.Length; i++)
                        {
                            args[i] = Translate(chunk, index, code.A + i, JitOperandFormat.DST);
                        }
                        return GlobalExpression.Assign(
                            args,
                            GlobalExpression.FunctionCall(
                                Translate(chunk, index, code.A, JitOperandFormat.VAR),
                                items
                                )
                            );
                    }
                case JitOperand.CALL:
                    {
                        var items = new Expression[code.D - 1];
                        for (int i = 0; i < items.Length; i++)
                        {
                            items[i] = Translate(chunk, index, code.A + i, JitOperandFormat.VAR);
                        }
                        var args = new Expression[code.B - 1];
                        for (int i = 0; i < args.Length; i++)
                        {
                            args[i] = Translate(chunk, index, code.A + i, JitOperandFormat.DST);
                        }
                        return GlobalExpression.Assign(
                            args,
                            GlobalExpression.FunctionCall(
                                Translate(chunk, index, code.A, JitOperandFormat.VAR),
                                items
                                )
                            );
                    }
                case JitOperand.CALLMT:
                    {
                        var items = new Expression[code.D - 1];
                        for (int i = 0; i < items.Length; i++)
                        {
                            items[i] = Translate(chunk, index, code.A + i, JitOperandFormat.VAR);
                        }
                        return GlobalExpression.Return([
                            GlobalExpression.FunctionCall(
                                Translate(chunk, index, code.A, JitOperandFormat.VAR),
                                [..items, Expression.Default(typeof(Array))]
                                )
                            ]);
                    }
                case JitOperand.CALLT:
                    {
                        var items = new Expression[code.D - 1];
                        for (int i = 0; i < items.Length; i++)
                        {
                            items[i] = Translate(chunk, index, code.A + i, JitOperandFormat.VAR);
                        }
                        return GlobalExpression.Return([
                            GlobalExpression.FunctionCall(
                                Translate(chunk, index, code.A, JitOperandFormat.VAR),
                                items
                                )
                            ]);
                    }
                case JitOperand.ITERC:
                case JitOperand.ITERN:
                    {
                        var A_minus_three = Translate(chunk, index, code.A - 3, JitOperandFormat.VAR);
                        var A_minus_two = Translate(chunk, index, code.A - 2, JitOperandFormat.VAR);
                        var A_minus_one = Translate(chunk, index, code.A - 1, JitOperandFormat.VAR);
                        var items = new Expression[code.B - 1];
                        for (int i = 0; i < items.Length; i++)
                        {
                            items[i] = Translate(chunk, index, code.A + i, JitOperandFormat.DST);
                        }
                        return Expression.Block(
                            GlobalExpression.Assign([
                                Translate(chunk, index, code.A, JitOperandFormat.DST),
                                Translate(chunk, index, code.A + 1, JitOperandFormat.DST),
                                Translate(chunk, index, code.A + 2, JitOperandFormat.DST),

                                ], A_minus_three, A_minus_two, A_minus_one),
                            GlobalExpression.Assign(
                                items,
                                GlobalExpression.FunctionCall(
                                    A_minus_three,
                                    [A_minus_two, A_minus_one]
                                    )
                                )
                        );
                    }
                case JitOperand.VARG:
                    {
                        var items = new Expression[code.B - 1];
                        for (int i = 0; i < items.Length; i++)
                        {
                            items[i] = Translate(chunk, index, code.A + i, JitOperandFormat.VAR);
                        }
                        return GlobalExpression.Return(items);
                    }
                case JitOperand.ISNEXT:
                    break;
                case JitOperand.RETM:
                    {
                        var items = new Expression[code.D - 1];
                        for (int i = 0; i < items.Length; i++)
                        {
                            items[i] = Translate(chunk, index, code.A + i, JitOperandFormat.VAR);
                        }
                        return GlobalExpression.Return(items);
                    }
                case JitOperand.RET:
                    {
                        var items = new Expression[code.D - 2];
                        for (int i = 0; i < items.Length; i++)
                        {
                            items[i] = Translate(chunk, index, code.A + i, JitOperandFormat.VAR);
                        }
                        return GlobalExpression.Return(items);
                    }
                case JitOperand.RET0:
                    return GlobalExpression.Return([]);
                case JitOperand.RET1:
                    return GlobalExpression.Return([Translate(chunk, index, code.A, attr.AType)]);
                case JitOperand.FORI:
                case JitOperand.JFORI:
                    {
                        var label = Expression.Label(typeof(int));
                        var result = Translate(chunk, index, code.A + 3, JitOperandFormat.VAR);
                        return Expression.Block(
                            result,
                            Expression.Assign(
                                result,
                                Translate(chunk, index, code.A, JitOperandFormat.BS)
                                ),
                            Expression.Loop(
                                Expression.IfThenElse(
                                    Translate(chunk, index, code.A + 1, JitOperandFormat.BS),
                                    Translate(chunk, index, code.A + 2, JitOperandFormat.BS),
                                    Expression.Break(label, result)
                                )
                            )
                        );
                    }
                case JitOperand.FORL:
                case JitOperand.IFORL:
                case JitOperand.JFORL:
                    {
                        var a = Translate(chunk, index, code.A + 3, JitOperandFormat.VAR);
                        var two = Translate(chunk, index, code.A + 2, JitOperandFormat.VAR);
                        return Expression.Block(
                            Expression.AddAssign(
                                a,
                                two
                                ),
                            Expression.IfThen(
                                GlobalExpression.FunctionCall("cmp", a, two, Translate(chunk, index, code.A + 1, JitOperandFormat.VAR)),
                                Expression.Goto(
                                    Expression.Label()
                                //Translate(chunk, index, (int)code.D, JitOperandFormat.JMP)
                                )
                            )
                        );
                    }
                case JitOperand.ITERL:
                case JitOperand.IITERL:
                case JitOperand.JITERL:
                    {
                        var a = Translate(chunk, index, code.A, JitOperandFormat.VAR);
                        return Expression.Block(
                            Expression.Assign(
                                Translate(chunk, index, code.A - 1, JitOperandFormat.VAR),
                                a           
                            ),
                            Expression.IfThen(
                                Expression.NotEqual(a, Expression.Constant(null)),
                                Expression.Goto(Expression.Label(
                                    // Translate(chunk, index, (int)code.D, JitOperandFormat.JMP)
                                ))
                            )
                        );
                    }
                case JitOperand.LOOP:
                    break;
                case JitOperand.ILOOP:
                    break;
                case JitOperand.JLOOP:
                    break;
                case JitOperand.JMP:
                    break;
                case JitOperand.FUNCF:
                    break;
                case JitOperand.IFUNCF:
                    break;
                case JitOperand.JFUNCF:
                    break;
                case JitOperand.FUNCV:
                    break;
                case JitOperand.IFUNCV:
                    break;
                case JitOperand.JFUNCV:
                    break;
                case JitOperand.FUNCC:
                    break;
                case JitOperand.FUNCCW:
                    break;
                case JitOperand.UNKNW:
                    break;
                default:
                    return Expression.Constant(null);
            }
            return Expression.Constant(null);
        }
        private string TranslateName(LuaChunk chunk,
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
            if (format == JitOperandFormat.STR)
            {
                return chunk.ConstantItems[value].Value.ToString();
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
            if (format is JitOperandFormat.BS or JitOperandFormat.RBS)
            {
                return $"r{value}";
            }
            return string.Empty;
        }
        private Expression Translate(LuaChunk chunk, 
            int index, int value, 
            JitOperandFormat format)
        {
            if (format is JitOperandFormat.DST or JitOperandFormat.BS)
            {
                return Expression.Variable(typeof(string), $"slot{value}");
            }
            if (format == JitOperandFormat.VAR)
            {
                return Expression.Variable(typeof(string), $"slot{value}");
            }
            if (format == JitOperandFormat.UV)
            {
                var name = chunk.DebugInfo.UpValueNameItems[value];
                return Expression.Variable(typeof(string), $"slot{value}\"${name}\"");
            }
            if (format == JitOperandFormat.PRI)
            {
                return value switch
                {
                    0 => Expression.Constant(null),
                    2 => Expression.Constant(true),
                    _ => Expression.Constant(false),
                };
            }
            if (format == JitOperandFormat.NUM)
            {
                return Expression.Constant(chunk.NumberConstantItems[value].Value);
            }
            if (format == JitOperandFormat.STR)
            {
                return Expression.Constant(chunk.ConstantItems[value].Value);
            }
            if (format == JitOperandFormat.TAB)
            {
                return Expression.Variable(typeof(string), $"table#k{value}");
            }
            if (format == JitOperandFormat.CDT)
            {
                return Expression.Constant(chunk.ConstantItems[value].Value);
            }
            if (format == JitOperandFormat.JMP)
            {
                return Expression.Variable(typeof(string), $"{1 + index + value}");
            }
            if (format is JitOperandFormat.LIT or JitOperandFormat.SLIT)
            {
                return Expression.Constant(value);
            }
            if (format is JitOperandFormat.BS or JitOperandFormat.RBS)
            {
                return Expression.Variable(typeof(string), $"r{value}");
            }
            return Expression.Constant(null);
        }
    }
}
