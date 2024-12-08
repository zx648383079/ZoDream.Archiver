namespace ZoDream.LuaDecompiler.Models
{
    public class JitOperandCode(JitOperand op, byte a) : IOperandCode
    {
        public JitOperand Operand { get; private set; } = op;

        public byte A { get; private set; } = a;
        public byte B { get; internal set; }
        public byte C { get; internal set; }
        public uint D { get; internal set; }
    }
}
