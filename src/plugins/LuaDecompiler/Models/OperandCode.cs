namespace ZoDream.LuaDecompiler.Models
{
    public class OperandCode(Operand op, int code, OperandExtractor extractor) : IOperandCode
    {
        public Operand Operand { get; set; } = op;

        public int A => extractor.A.Extract(code);
        public int B => extractor.B.Extract(code);
        public int sB => B - extractor.B.Max / 2;
        public int C => extractor.C.Extract(code);
        public int sC => C - extractor.C.Max / 2;

        public bool k => extractor.k.Extract(code) != 0;

        public int Ax => extractor.Ax.Extract(code);
        public int Bx => extractor.Bx.Extract(code);
        public int sBx => extractor.sBx.Extract(code);

    }
}
