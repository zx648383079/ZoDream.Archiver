using System;
using System.Collections.Generic;
using System.Text;
using ZoDream.ShaderDecompiler.SpirV;

namespace ZoDream.ShaderDecompiler
{
    public partial class SpirVWriter
    {

        private void Disassemble(StringBuilder builder)
        {
            Disassemble(builder, DisassemblyOptions.Default);
        }

        private void Disassemble(StringBuilder builder, DisassemblyOptions options)
        {
            builder.AppendLine("; SPIR-V");
            builder.Append("; Version: ").Append(_header.Version).AppendLine();
            var generator = SpvGenerator.Items[_header.GeneratorId];
            if (generator is null)
            {
                builder.Append("; Generator: unknown; ").Append(_header.GeneratorVersion).AppendLine();
            }
            else
            {
                builder.Append("; Generator: ").Append(generator.Vendor).Append(' ').
                    Append(generator.Name).Append("; ").Append(_header.GeneratorVersion).AppendLine();
            }
            builder.Append("; Bound: ").Append(_header.Bound).AppendLine();
            builder.Append("; Schema: ").Append(_header.Reserved).AppendLine();

            var lines = new string[_instructions.Length + 1];
            lines[0] = builder.ToString();
            builder.Clear();

            for (int i = 0; i < _instructions.Length; i++)
            {
                var instruction = _instructions[i];
                PrintInstruction(builder, instruction, options);
                lines[i + 1] = builder.ToString();
                builder.Clear();
            }

            int longestPrefix = 0;
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                longestPrefix = Math.Max(longestPrefix, line.IndexOf('='));
                if (longestPrefix > 50)
                {
                    longestPrefix = 50;
                    break;
                }
            }

            builder.Append(lines[0]);
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];
                int index = line.IndexOf('=');
                if (index == -1)
                {
                    builder.Append(' ', longestPrefix + 4);
                    builder.Append(line);
                }
                else
                {
                    int pad = Math.Max(0, longestPrefix - index);
                    builder.Append(' ', pad);
                    builder.Append(line, 0, index);
                    builder.Append('=');
                    builder.Append(line, index + 1, line.Length - index - 1);
                }
                builder.AppendLine();
            }
        }

        private static void PrintInstruction(StringBuilder sb, ParsedInstruction instruction, DisassemblyOptions options)
        {
            if (instruction.Operands.Count == 0)
            {
                sb.Append(instruction.Instruction.Mnemonic);
                return;
            }

            int currentOperand = 0;
            if (instruction.Instruction.Operands[currentOperand].Type is IdResultType)
            {
                if (options.HasFlag(DisassemblyOptions.ShowTypes))
                {
                    var resultType = instruction.ResultType ?? throw new NullReferenceException();
                    resultType.ToString(sb).Append(' ');
                }
                ++currentOperand;
            }

            if (currentOperand < instruction.Operands.Count && instruction.Instruction.Operands[currentOperand].Type is IdResult)
            {
                if (!options.HasFlag(DisassemblyOptions.ShowNames) || string.IsNullOrWhiteSpace(instruction.Name))
                {
                    PrintOperandValue(sb, instruction.Operands[currentOperand].Value, options);
                }
                else
                {
                    sb.Append(instruction.Name);
                }
                sb.Append(" = ");

                ++currentOperand;
            }

            sb.Append(instruction.Instruction.Mnemonic);
            sb.Append(' ');

            for (; currentOperand < instruction.Operands.Count; ++currentOperand)
            {
                PrintOperandValue(sb, instruction.Operands[currentOperand].Value, options);
                sb.Append(' ');
            }
        }

        private static void PrintOperandValue(StringBuilder sb, object value, DisassemblyOptions options)
        {
            switch (value)
            {
                case Type t:
                    sb.Append(t.Name);
                    break;

                case string s:
                    {
                        sb.Append('"');
                        sb.Append(s);
                        sb.Append('"');
                    }
                    break;

                case ObjectReference or:
                    {
                        if (options.HasFlag(DisassemblyOptions.ShowNames) && or.Reference != null && !string.IsNullOrWhiteSpace(or.Reference.Name))
                        {
                            sb.Append(or.Reference.Name);
                        }
                        else
                        {
                            or.ToString(sb);
                        }
                    }
                    break;

                case IBitEnumOperandValue beov:
                    PrintBitEnumValue(sb, beov, options);
                    break;

                case IValueEnumOperandValue veov:
                    PrintValueEnumValue(sb, veov, options);
                    break;

                case VaryingOperandValue varOpVal:
                    varOpVal.ToString(sb);
                    break;

                default:
                    sb.Append(value);
                    break;
            }
        }

        private static void PrintBitEnumValue(StringBuilder sb, IBitEnumOperandValue enumOperandValue, DisassemblyOptions options)
        {
            foreach (uint key in enumOperandValue.Values.Keys)
            {
                sb.Append(enumOperandValue.GetEnumName(key));
                IReadOnlyList<object> value = enumOperandValue.Values[key];
                if (value.Count != 0)
                {
                    sb.Append(' ');
                    foreach (object v in value)
                    {
                        PrintOperandValue(sb, v, options);
                    }
                }
            }
        }

        private static void PrintValueEnumValue(StringBuilder sb, IValueEnumOperandValue valueOperandValue, DisassemblyOptions options)
        {
            sb.Append(valueOperandValue.Key);
            if (valueOperandValue.Value is IList<object> valueList && valueList.Count > 0)
            {
                sb.Append(' ');
                foreach (object v in valueList)
                {
                    PrintOperandValue(sb, v, options);
                }
            }
        }
    }
}
