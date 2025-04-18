﻿using System.Collections.Generic;
using System.IO;
using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Language;
using ZoDream.Shared.Models;

namespace ZoDream.LuaDecompiler
{
    public partial class LuaWriter(LuaBytecode data) : ILanguageWriter, IDisassembler
    {
        private LuaVersion Version => data.Header.Version;

        public void Write(Stream output)
        {
            using var writer = new CodeWriter(output);
            Decompile(writer);
        }

        public void Decompile(ICodeWriter writer, IInstruction instruction)
        {
            if (instruction is LuaChunk chunk)
            {
                Translate(writer, chunk, chunk.CurrentOpcode);
            }
        }

        public void Decompile(ICodeWriter writer)
        {
            writer.WriteLine("--")
                .Write("-- Disassemble of ").WriteLine()
                .WriteLine("--")
                .Write("-- Source file: ").WriteLine("N/A")
                .WriteLine("--")
                .WriteLine("-- Flags:")
                .Write("--    Stripped: ").WriteLine(data.Header.Flags?.IsStripped == true ? "Yes" : "No")
                .Write("--    Endianness: ").WriteLine(data.Header.Endianness == EndianType.BigEndian ? "Big" : "Little")
                .Write("--    FFI: ").WriteLine(data.Header.Flags?.HasFFI == true ? "Present" : "Not present")
                .WriteLine("--")
                .WriteLine();

            writer.Write("-- main ");
            Decompile(writer, data.MainChunk);
        }

        private void Decompile(ICodeWriter writer, LuaChunk chunk)
        {
            writer.WriteFormat("{0}:{1}-{2}: {3} {4} args, {5} upvalues, {6} slots",
                data.MainChunk.Name,
                chunk.LineDefined,
                chunk.LastLineDefined, chunk.ParameterCount, 
                chunk.Flags?.IsVariadic == true ? "+" : string.Empty,
                chunk.UpValueCount, chunk.MaxStack).WriteLine();

            writer.WriteIndent().WriteLine("--[[ -- constant tables --");
            for (int i = 0; i < chunk.ConstantItems.Length; i++)
            {
                var item = chunk.ConstantItems[i];
                if (item.Type != LuaConstantType.Table)
                {
                    continue;
                }
                Translate(writer, i, (LuaConstantTable)item.Value);
            }
            writer.WriteIndent().WriteLine("--]]");
            writer.WriteIndent().WriteLine("-- instructions --");
            while (chunk.MoveNext())
            {
                Translate(writer, chunk, chunk.CurrentOpcode);
            }
            _envItems.RemoveStartWith("slot");
        }

        private void Translate(ICodeWriter writer, LuaChunk chunk, ILanguageOpcode item)
        {
            writer.WriteIndent();
            switch (item)
            {
                case JitOperandCode jit:
                    Translate(writer, chunk, jit);
                    break;
                case OperandCode code:
                    Translate(writer, chunk, code);
                    break;
            }
            writer.WriteLine(
#if DEBUG
                $"        -- [{chunk.CurrentIndex}] {item}"
#endif
                );
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
                writer.Write("#").Write(i).Write(": ").Write(TranslateConstant(item)).Write(",");
            }
            foreach (var item in table.HashItems)
            {
                writer.Write("[").Write(TranslateConstant(item.Item1))
                    .Write("] = ").Write(TranslateConstant(item.Item2)).Write(",");
            }
            writer.Write(']').WriteLine();
        }


        public IEnumerable<IInstruction> Disassemble()
        {
            var chunk = data.MainChunk;
            while (chunk.MoveNext())
            {
                yield return chunk;
            }
        }
    }
}
