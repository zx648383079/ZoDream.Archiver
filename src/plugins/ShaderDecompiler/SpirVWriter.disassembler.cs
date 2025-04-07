using System;
using System.Collections.Generic;
using ZoDream.ShaderDecompiler.SpirV;
using ZoDream.Shared.Language;

namespace ZoDream.ShaderDecompiler
{
    public partial class SpirVWriter
    {

        private void Disassemble(ICodeWriter builder, DisassemblyOptions options)
        {
            builder.WriteLine("; SPIR-V")
                .Write("; Version: ")
                .Write(_header.Version)
                .WriteLine();
            var generator = SpvGenerator.Items[_header.GeneratorId];
            if (generator is null)
            {
                builder.Write("; Generator: unknown; ")
                    .Write(_header.GeneratorVersion)
                    .WriteLine();
            }
            else
            {
                builder.Write("; Generator: ")
                    .Write(generator.Vendor)
                    .Write(' ')
                    .Write(generator.Name)
                    .Write("; ")
                    .Write(_header.GeneratorVersion)
                    .WriteLine();
            }
            builder.Write("; Bound: ")
                .Write(_header.Bound)
                .WriteLine()
                .Write("; Schema: ")
                .Write(_header.Reserved)
                .WriteLine();
            foreach (var item in _instructions)
            {
                builder.WriteIndent();
                PrintInstruction(builder, item, options);
                builder.WriteLine();
            }
        }

        private static void PrintInstruction(ICodeWriter builder, ParsedInstruction instruction, DisassemblyOptions options)
        {
            if (instruction.Operands.Count == 0)
            {
                builder.Write(instruction.Instruction.Mnemonic);
                return;
            }

            int currentOperand = 0;
            if (instruction.Instruction.Operands[currentOperand].Type is IdResultType)
            {
                if (options.HasFlag(DisassemblyOptions.ShowTypes))
                {
                    var resultType = instruction.ResultType ?? throw new NullReferenceException();
                    resultType.Write(builder);
                    builder.Write(' ');
                }
                ++currentOperand;
            }

            if (currentOperand < instruction.Operands.Count && instruction.Instruction.Operands[currentOperand].Type is IdResult)
            {
                if (!options.HasFlag(DisassemblyOptions.ShowNames) || string.IsNullOrWhiteSpace(instruction.Name))
                {
                    PrintOperandValue(builder, instruction.Operands[currentOperand].Value, options);
                }
                else
                {
                    builder.Write(instruction.Name);
                }
                builder.Write(" = ");

                ++currentOperand;
            }

            builder.Write(instruction.Instruction.Mnemonic)
                .Write(' ');

            for (; currentOperand < instruction.Operands.Count; ++currentOperand)
            {
                PrintOperandValue(builder, instruction.Operands[currentOperand].Value, options);
                builder.Write(' ');
            }
        }

        private static void PrintOperandValue(ICodeWriter builder, object value, DisassemblyOptions options)
        {
            switch (value)
            {
                case Type t:
                    builder.Write(t.Name);
                    break;

                case string s:
                    {
                        builder.Write(s, true);
                    }
                    break;

                case ObjectReference or:
                    {
                        if (options.HasFlag(DisassemblyOptions.ShowNames) && or.Reference != null && !string.IsNullOrWhiteSpace(or.Reference.Name))
                        {
                            builder.Write(or.Reference.Name);
                        }
                        else
                        {
                            or.Write(builder);
                        }
                    }
                    break;

                case IBitEnumOperandValue beov:
                    PrintBitEnumValue(builder, beov, options);
                    break;

                case IValueEnumOperandValue veov:
                    PrintValueEnumValue(builder, veov, options);
                    break;

                case VaryingOperandValue varOpVal:
                    varOpVal.Write(builder);
                    break;

                default:
                    builder.Write(value);
                    break;
            }
        }

        private static void PrintBitEnumValue(ICodeWriter sb, IBitEnumOperandValue enumOperandValue, DisassemblyOptions options)
        {
            foreach (uint key in enumOperandValue.Values.Keys)
            {
                sb.Write(enumOperandValue.GetEnumName(key));
                IReadOnlyList<object> value = enumOperandValue.Values[key];
                if (value.Count != 0)
                {
                    sb.Write(' ');
                    foreach (object v in value)
                    {
                        PrintOperandValue(sb, v, options);
                    }
                }
            }
        }

        private static void PrintValueEnumValue(ICodeWriter sb, IValueEnumOperandValue valueOperandValue, DisassemblyOptions options)
        {
            sb.Write(valueOperandValue.Key);
            if (valueOperandValue.Value is IList<object> valueList && valueList.Count > 0)
            {
                sb.Write(' ');
                foreach (object v in valueList)
                {
                    PrintOperandValue(sb, v, options);
                }
            }
        }
    }
}
