using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq.Expressions;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Language;
using ZoDream.Shared.Models;

namespace ZoDream.LuaDecompiler
{
    public partial class LuaWriter(LuaBytecode data) : ILanguageWriter, IDisassembler
    {

        public void Write(Stream output)
        {
            using var writer = new CodeWriter(output);
            Decompile(writer);
        }

        public void Decompile(ICodeWriter writer, IInstruction instruction)
        {
        }

        public void Decompile(ICodeWriter writer)
        {
            writer.WriteLine(";")
                .Write("; Disassemble of ").WriteLine()
                .WriteLine(";")
                .Write(";  Source file: ").WriteLine("N/A")
                .WriteLine(";")
                .WriteLine("; Flags:")
                .Write(";    Stripped: ").WriteLine(data.Header.Flags?.IsStripped == true ? "Yes" : "No")
                .Write(";    Endianness: ").WriteLine(data.Header.Endianness == EndianType.BigEndian ? "Big" : "Little")
                .Write(";    FFI: ").WriteLine(data.Header.Flags?.HasFFI == true ? "Present" : "Not present")
                .WriteLine(";")
                .WriteLine();

            writer.Write("main ");
            Decompile(writer, data.MainChunk);
        }

        private void Decompile(ICodeWriter writer, LuaChunk chunk)
        {
            writer.Write(data.MainChunk.Name).Write(":")
                .Write(chunk.LineDefined).Write("-")
                .Write(chunk.LastLineDefined).Write(": ")
                .Write(chunk.ParameterCount).Write(chunk.Flags?.IsVariadic == true ? "+" : string.Empty)
                .Write(" args, ").Write(chunk.UpValueCount).Write(" upvalues, ")
                .Write(chunk.MaxStack)
                .WriteLine(" slots");

            writer.WriteLine(";;;; constant tables ;;;;");
            for (int i = 0; i < chunk.ConstantItems.Length; i++)
            {
                var item = chunk.ConstantItems[i];
                if (item.Type != LuaConstantType.Table)
                {
                    continue;
                }
                Translate(writer, i, (LuaConstantTable)item.Value);
            }
            writer.WriteLine(";;;; instructions ;;;;");

            var index = 0;
            foreach (var item in chunk.OpcodeItems)
            {
                index++;
                switch(item)
                {
                    case JitOperandCode jit:
                        if (jit.Operand == JitOperand.FNEW)
                        {
                            writer.Write(index).Write(" ").Write(chunk.DebugInfo.LineNoItems[index])
                            .Write(" ")
                            .Write("FNEW ").Write(jit.A).Write("  ").Write(jit.D).Write(" ; ");
                            Decompile(writer, chunk.PrototypeItems[jit.D]);
                        } else
                        {
                            Translate(writer, chunk, index, jit);
                        }
                        break;
                    case OperandCode code:
                        Translate(writer, chunk, index, code);
                        break;
                }
            }
        }

        private void Translate(ICodeWriter writer, int index, LuaConstantTable table)
        {
            writer.Write("ktable#").Write(index).Write(" = [");
            for (int i = 1; i < table.Items.Length; i++)
            {
                var item = table.Items[i];
                if (item.Type == LuaConstantType.Null)
                {
                    continue;
                }
                writer.Write("#").Write(i).Write(": ").Write(Translate(item)).Write(",");
            }
            foreach (var item in table.HashItems)
            {
                writer.Write("[").Write(Translate(item.Item1))
                    .Write("] = ").Write(Translate(item.Item2)).Write(",");
            }
            writer.Write(']').WriteLine();
        }

        private string? Translate(LuaConstant item)
        {
            return item.Type switch
            {
                LuaConstantType.Null => "nil",
                LuaConstantType.Bool => (bool)item.Value == true ? "true" : "false",
                _ => (string)item.Value,
            };
        }

        public IEnumerable<IInstruction> Disassemble()
        {
            throw new System.NotImplementedException();
        }
    }
}
