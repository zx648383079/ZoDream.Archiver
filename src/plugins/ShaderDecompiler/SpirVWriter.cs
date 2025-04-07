using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using ZoDream.ShaderDecompiler.SpirV;
using ZoDream.Shared.Language;

namespace ZoDream.ShaderDecompiler
{
    public partial class SpirVWriter : ILanguageWriter, IDisassembler
    {
        private static readonly SpvOperand[] _debugInstructions =
        [
            SpvOperand.SourceContinued,
            SpvOperand.Source,
            SpvOperand.SourceExtension,
            SpvOperand.Name,
            SpvOperand.MemberName,
            SpvOperand.String,
            SpvOperand.Line,
            SpvOperand.NoLine,
            SpvOperand.ModuleProcessed,
        ];

        public SpirVWriter(SpvBytecode data)
        {
            _header = data.Header;
            _instructions = [.. data.MainChunk.OpcodeItems.Select(item => new ParsedInstruction(Convert(item.Operand), item.Data))];
            ProcessInstruction();
        }

        private readonly SpvHeader _header;
        private readonly ParsedInstruction[] _instructions = [];
        private readonly Dictionary<uint, ParsedInstruction> _instructionMaps = [];

        public IEnumerable<IInstruction> Disassemble()
        {
            foreach (var item in _instructions)
            {
                yield return item;
            }
        }

        public void Decompile(ICodeWriter writer, IInstruction instruction)
        {
            if (instruction is ParsedInstruction item)
            {
                Decompile(writer, item);
            }
        }

        public void Write(Stream output)
        {
            using var writer = new CodeWriter(output);
            Disassemble(writer, DisassemblyOptions.Default);
        }

        public void Decompile(ICodeWriter writer)
        {
            Disassemble(writer, DisassemblyOptions.Default);
        }

        public void Decompile(ICodeWriter writer, SpvOperandCode code)
        {
            Decompile(writer, new ParsedInstruction(Convert(code.Operand), code.Data));
        }

        public void Decompile(ICodeWriter writer, ParsedInstruction instruction)
        {
            PrintInstruction(writer, instruction, DisassemblyOptions.Default);
        }

        private void ProcessInstruction()
        {
            // Debug instructions can be only processed after everything
            // else has been parsed, as they may reference types which haven't
            // been seen in the file yet
            var debugInstructions = new List<ParsedInstruction>();
            // Entry points contain forward references
            // Those need to be resolved afterwards
            var entryPoints = new List<ParsedInstruction>();

            foreach (var instruction in _instructions)
            {
                if (_debugInstructions.Contains(instruction.Operand))
                {
                    debugInstructions.Add(instruction);
                    continue;
                }
                if (instruction.Operand == SpvOperand.EntryPoint)
                {
                    entryPoints.Add(instruction);
                    continue;
                }

                if (Enum.GetName(instruction.Operand)?.StartsWith("Type", StringComparison.Ordinal) ?? false)
                {
                    ProcessTypeInstruction(instruction);
                }

                instruction.ResolveResultType(_instructionMaps);
                if (instruction.HasResult)
                {
                    _instructionMaps[instruction.ResultId] = instruction;
                }

                switch (instruction.Operand)
                {
                    // Constants require that the result type has been resolved
                    case SpvOperand.SpecConstant:
                    case SpvOperand.Constant:

                        {
                            var t = instruction.ResultType;
                            Debug.Assert(t != null);
                            Debug.Assert(t is SpvScalarType);

                            object? constant = ConvertConstant(t as SpvScalarType, instruction.Words, 3);
                            instruction.Operands[2].Value = constant;
                            instruction.Value = constant;
                        }
                        break;
                }
            }

            foreach (var instruction in debugInstructions)
            {
                switch (instruction.Operand)
                {
                    case SpvOperand.MemberName:

                        {
                            var t = (SpvStructType?)_instructionMaps[instruction.Words[1]].ResultType ?? throw new NullReferenceException();
                            t.SetMemberName((uint)instruction.Operands[1].Value, (string)instruction.Operands[2].Value);
                        }
                        break;

                    case SpvOperand.Name:

                        {
                            // We skip naming objects we don't know about
                            ParsedInstruction t = _instructionMaps[instruction.Words[1]];
                            t.Name = (string)instruction.Operands[1].Value;
                        }
                        break;
                }
            }

            foreach (var instruction in _instructions)
            {
                instruction.ResolveReferences(_instructionMaps);
            }
        }

        private static object? ConvertConstant(SpvScalarType? type, IReadOnlyList<uint> words, int index)
        {
            switch (type)
            {
                case SpvIntegerType i:
                    {
                        if (i.Signed)
                        {
                            if (i.Width == 16)
                            {
                                return unchecked((short)words[index]);
                            }
                            else if (i.Width == 32)
                            {
                                return unchecked((int)words[index]);
                            }
                            else if (i.Width == 64)
                            {
                                return unchecked((long)(words[index] | ((ulong)words[index + 1] << 32)));
                            }
                        }
                        else
                        {
                            if (i.Width == 16)
                            {
                                return unchecked((ushort)words[index]);
                            }
                            else if (i.Width == 32)
                            {
                                return words[index];
                            }
                            else if (i.Width == 64)
                            {
                                return words[index] | ((ulong)words[index + 1] << 32);
                            }
                        }

                        throw new Exception("Cannot construct integer literal.");
                    }

                case SpvFloatingPointType f:
                    {
                        if (f.Width == 32)
                        {
                            uint value = words[0];
                            return Unsafe.As<uint, float>(ref value);
                        }
                        else if (f.Width == 64)
                        {
                            ulong value = words[index] | ((ulong)words[index + 1] << 32);
                            return Unsafe.As<ulong, double>(ref value);
                        }
                        else
                        {
                            throw new Exception("Cannot construct floating point literal.");
                        }
                    }
                default:
                    return null;
            }
        }

        private void ProcessTypeInstruction(ParsedInstruction i)
        {
            switch (i.Operand)
            {
                case SpvOperand.TypeInt:
                    {
                        i.ResultType = new SpvIntegerType((int)i.Words[2], i.Words[3] == 1u);
                    }
                    break;

                case SpvOperand.TypeFloat:
                    {
                        i.ResultType = new SpvFloatingPointType((int)i.Words[2]);
                    }
                    break;

                case SpvOperand.TypeVector:
                    {
                        i.ResultType = new SpvVectorType((SpvScalarType)_instructionMaps[i.Words[2]].ResultType, (int)i.Words[3]);
                    }
                    break;

                case SpvOperand.TypeMatrix:
                    {
                        i.ResultType = new SpvMatrixType((SpvVectorType)_instructionMaps[i.Words[2]].ResultType, (int)i.Words[3]);
                    }
                    break;

                case SpvOperand.TypeArray:
                    {
                        object? constant = _instructionMaps[i.Words[3]].Value;
                        int size = 0;

                        switch (constant)
                        {
                            case ushort u16:
                                size = u16;
                                break;

                            case uint u32:
                                size = (int)u32;
                                break;

                            case ulong u64:
                                size = (int)u64;
                                break;

                            case short i16:
                                size = i16;
                                break;

                            case int i32:
                                size = i32;
                                break;

                            case long i64:
                                size = (int)i64;
                                break;
                        }

                        i.ResultType = new SpvArrayType(_instructionMaps[i.Words[2]].ResultType, size);
                    }
                    break;

                case SpvOperand.TypeRuntimeArray:
                    {
                        i.ResultType = new SpvRuntimeArrayType((SpvType)_instructionMaps[i.Words[2]].ResultType);
                    }
                    break;

                case SpvOperand.TypeBool:
                    {
                        i.ResultType = new SpvBoolType();
                    }
                    break;

                case SpvOperand.TypeOpaque:
                    {
                        i.ResultType = new SpvOpaqueType();
                    }
                    break;

                case SpvOperand.TypeVoid:
                    {
                        i.ResultType = new SpvVoidType();
                    }
                    break;

                case SpvOperand.TypeImage:
                    {
                        var sampledType = _instructionMaps[i.Operands[1].GetId()].ResultType;
                        Dim dim = i.Operands[2].GetSingleEnumValue<Dim>();
                        uint depth = (uint)i.Operands[3].Value;
                        bool isArray = (uint)i.Operands[4].Value != 0;
                        bool isMultiSampled = (uint)i.Operands[5].Value != 0;
                        uint sampled = (uint)i.Operands[6].Value;
                        ImageFormat imageFormat = i.Operands[7].GetSingleEnumValue<ImageFormat>();

                        i.ResultType = new SpvImageType(sampledType,
                            dim,
                            (int)depth, isArray, isMultiSampled,
                            (int)sampled, imageFormat,
                            i.Operands.Count > 8 ? i.Operands[8].GetSingleEnumValue<AccessQualifier>() : AccessQualifier.ReadOnly);
                    }
                    break;

                case SpvOperand.TypeSampler:
                    {
                        i.ResultType = new SpvSamplerType();
                        break;
                    }

                case SpvOperand.TypeSampledImage:
                    {
                        i.ResultType = new SpvSampledImageType((SpvImageType)_instructionMaps[i.Words[2]].ResultType);
                    }
                    break;

                case SpvOperand.TypeFunction:
                    {
                        var parameterTypes = new List<SpvType>();
                        for (int j = 3; j < i.Words.Count; ++j)
                        {
                            parameterTypes.Add(_instructionMaps[i.Words[j]].ResultType);
                        }
                        i.ResultType = new SpvFunctionType(_instructionMaps[i.Words[2]].ResultType, parameterTypes);
                    }
                    break;

                case SpvOperand.TypeForwardPointer:
                    {
                        // We create a normal pointer, but with unspecified type
                        // This will get resolved later on
                        i.ResultType = new SpvPointerType((StorageClass)i.Words[2]);
                    }
                    break;

                case SpvOperand.TypePointer:
                    {
                        if (_instructionMaps.ContainsKey(i.Words[1]))
                        {
                            // If there is something present, it must have been
                            // a forward reference. The storage type must
                            // match
                            var pt = (SpvPointerType)i.ResultType;
                            Debug.Assert(pt != null);
                            Debug.Assert(pt.StorageClass == (StorageClass)i.Words[2]);
                            pt.ResolveForwardReference(_instructionMaps[i.Words[3]].ResultType);
                        }
                        else
                        {
                            i.ResultType = new SpvPointerType((StorageClass)i.Words[2], _instructionMaps[i.Words[3]].ResultType);
                        }
                    }
                    break;

                case SpvOperand.TypeStruct:
                    {
                        var memberTypes = new List<SpvType>();
                        for (int j = 2; j < i.Words.Count; ++j)
                        {
                            memberTypes.Add(_instructionMaps[i.Words[j]].ResultType);
                        }
                        i.ResultType = new SpvStructType(memberTypes);
                    }
                    break;
            }
        }


    }
}
