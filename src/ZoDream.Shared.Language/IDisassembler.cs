using System.Collections.Generic;

namespace ZoDream.Shared.Language
{
    public interface IDisassembler
    {
        public IEnumerable<IInstruction> Disassemble();

        public string Decompile(IInstruction instruction);
    }
}
