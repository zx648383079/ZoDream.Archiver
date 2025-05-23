﻿using System;
using System.Collections.Generic;
using System.Text;
using ZoDream.Shared.Language;

namespace ZoDream.ShaderDecompiler.SpirV
{
    public class ParsedOperand : IInstructionOperand
    {
        public ParsedOperand(IReadOnlyList<uint> words, int index, int count, object value, SpvInstructionOperand operand)
        {
            uint[] array = new uint[count];
            for (int i = 0; i < count; i++)
            {
                array[i] = words[index + i];
            }

            Words = array;
            Value = value;
            Operand = operand;
        }

        public T GetSingleEnumValue<T>() where T : Enum
        {
            IValueEnumOperandValue v = (IValueEnumOperandValue)Value;
            if (v.Value.Count == 0)
            {
                // If there's no value at all, the enum is probably something like ImageFormat.
                // In which case we just return the enum value
                return (T)v.Key;
            }
            else
            {
                // This means the enum has a value attached to it, so we return the attached value
                return (T)((IValueEnumOperandValue)Value).Value[0];
            }
        }

        public uint GetId()
        {
            return ((ObjectReference)Value).Id;
        }

        public uint GetBitEnumValue()
        {
            IBitEnumOperandValue v = (IBitEnumOperandValue)Value;

            uint result = 0;
            foreach (uint k in v.Values.Keys)
            {
                result |= k;
            }

            return result;
        }

        public IReadOnlyList<uint> Words { get; }
        public object Value { get; set; }
        public SpvInstructionOperand Operand { get; }
    }

    public class VaryingOperandValue
    {
        public VaryingOperandValue(IReadOnlyList<object> values)
        {
            Values = values;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            Write(new CodeWriter(sb));
            return sb.ToString();
        }

        public void Write(ICodeWriter sb)
        {
            for (int i = 0; i < Values.Count; ++i)
            {
                if (Values[i] is ObjectReference objRef)
                {
                    objRef.Write(sb);
                }
                else
                {
                    sb.Write(Values[i]);
                }
                if (i < (Values.Count - 1))
                {
                    sb.Write(' ');
                }
            }
        }

        public IReadOnlyList<object> Values { get; }
    }

    public interface IEnumOperandValue
    {
        string? GetEnumName(uint value);
    }

    public interface IBitEnumOperandValue : IEnumOperandValue
    {
        IReadOnlyDictionary<uint, IReadOnlyList<object>> Values { get; }
    }

    public interface IValueEnumOperandValue : IEnumOperandValue
    {
        object Key { get; }
        IReadOnlyList<object> Value { get; }
    }

    public abstract class EnumOperandValue<T> : IEnumOperandValue
        where T : unmanaged, Enum
    {
        public string? GetEnumName(uint value) => Enum.GetName((T)Enum.ToObject(typeof(T), value));
    }

    public class ValueEnumOperandValue<T> : EnumOperandValue<T>, IValueEnumOperandValue
        where T : unmanaged, Enum
    {
        public ValueEnumOperandValue(T key, IReadOnlyList<object> value)
        {
            Key = key;
            Value = value;
        }

        public object Key { get; }
        public IReadOnlyList<object> Value { get; }
    }

    public class BitEnumOperandValue<T> : EnumOperandValue<T>, IBitEnumOperandValue
        where T : unmanaged, Enum
    {
        public BitEnumOperandValue(Dictionary<uint, IReadOnlyList<object>> values)
        {
            Values = values;
        }

        public IReadOnlyDictionary<uint, IReadOnlyList<object>> Values { get; }
    }

    public class ObjectReference
    {
        public ObjectReference(uint id)
        {
            Id = id;
        }

        public void Resolve(IReadOnlyDictionary<uint, ParsedInstruction> objects)
        {
            Reference = objects[Id];
        }

        public override string ToString()
        {
            return $"%{Id}";
        }

        public void Write(ICodeWriter sb)
        {
            sb.Write('%').Write(Id);
        }

        public uint Id { get; }
        public ParsedInstruction? Reference { get; private set; }
    }

    public class ParsedInstruction : IInstruction
    {
        public ParsedInstruction(SpvInstruction instruction, 
            IReadOnlyList<uint> words)
        {
            Words = words;
            Instruction = instruction;
            ParseOperands();
        }

        private void ParseOperands()
        {
            if (Instruction.Operands.Length == 0)
            {
                return;
            }

            // Word 0 describes this instruction so we can ignore it
            int currentWord = 1;
            int currentOperand = 0;
            List<object> varyingOperandValues = [];
            int varyingWordStart = 0;
            SpvInstructionOperand? varyingOperand = null;

            while (currentWord < Words.Count && Instruction.Operands.Length > currentOperand)
            {
                var operand = Instruction.Operands[currentOperand];
                operand.Type.ReadValue(Words, currentWord, out object value, out int wordsUsed);
                if (operand.Quantifier == SpvOperandQuantifier.Varying)
                {
                    varyingOperandValues.Add(value);
                    varyingWordStart = currentWord;
                    varyingOperand = operand;
                }
                else
                {
                    int wordCount = Math.Min(Words.Count - currentWord, wordsUsed);
                    var parsedOperand = new ParsedOperand(Words, currentWord, wordCount, value, operand);
                    Operands.Add(parsedOperand);
                }

                currentWord += wordsUsed;
                if (operand.Quantifier != SpvOperandQuantifier.Varying)
                {
                    ++currentOperand;
                }
            }

            if (varyingOperand != null)
            {
                VaryingOperandValue varOperantValue = new VaryingOperandValue(varyingOperandValues);
                ParsedOperand parsedOperand = new ParsedOperand(Words, currentWord, Words.Count - currentWord, varOperantValue, varyingOperand);
                Operands.Add(parsedOperand);
            }
        }

        public void ResolveResultType(IReadOnlyDictionary<uint, ParsedInstruction> objects)
        {
            if (Instruction.Operands.Length > 0 && Instruction.Operands[0].Type is IdResultType)
            {
                ResultType = objects[(uint)Operands[0].Value].ResultType;
            }
        }

        public void ResolveReferences(IReadOnlyDictionary<uint, ParsedInstruction> objects)
        {
            foreach (ParsedOperand operand in Operands)
            {
                if (operand.Value is ObjectReference objectReference)
                {
                    objectReference.Resolve(objects);
                }
            }
        }

        public SpvType ResultType { get; set; }
        public uint ResultId {
            get {
                for (int i = 0; i < Instruction.Operands.Length; ++i)
                {
                    if (Instruction.Operands[i].Type is IdResult)
                    {
                        return Operands[i].GetId();
                    }
                }
                return 0;
            }
        }
        public bool HasResult => ResultId != 0;

        public IReadOnlyList<uint> Words { get; }
        public SpvInstruction Instruction { get; }

        public SpvOperand Operand => Instruction.Operand;
        public IList<ParsedOperand> Operands { get; } = [];
        public string? Name { get; set; }
        public object? Value { get; set; }

        public string Mnemonic => Instruction.Mnemonic;

        public IInstructionOperand[] OperandItems => [..Operands];
    }
}
