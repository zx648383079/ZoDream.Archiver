using ZoDream.Shared.Language;

namespace ZoDream.LuaDecompiler.Models
{
    public class JitOperandCode(JitOperand op, byte a) : ILanguageOpcode
    {
        public JitOperand Operand { get; private set; } = op;

        public byte A { get; private set; } = a;
        public byte B { get; internal set; }
        public byte C { get; internal set; }
        public uint D { get; internal set; }

        public override string ToString()
        {
            return $"{Operand}:{A},{B},{C},{D}";
        }
    }
}
