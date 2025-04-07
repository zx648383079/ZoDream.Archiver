using System.Collections.Generic;

namespace ZoDream.Shared.Language
{
    public interface IDisassembler
    {
        public IEnumerable<IInstruction> Disassemble();

        public void Decompile(ICodeWriter writer, IInstruction instruction);
        public void Decompile(ICodeWriter writer);
    }
}
