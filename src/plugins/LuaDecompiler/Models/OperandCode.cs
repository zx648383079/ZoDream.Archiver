using ZoDream.Shared.Language;

namespace ZoDream.LuaDecompiler.Models
{
    public class OperandCode(Operand op, int code, OperandExtractor extractor) : ILanguageOpcode, IInstructionOperand
    {
        public Operand Operand => op;

        public int Codepoint => code;

        public OperandExtractor Extractor => extractor;

        public int A => extractor.A.Extract(code);
        public int B => extractor.B.Extract(code);
        public int SB => B - extractor.B.Max / 2;
        public int C => extractor.C.Extract(code);
        public int SC => C - extractor.C.Max / 2;
        public int SJ => extractor.SJ.Extract(code);

        public int K => extractor.K.Extract(code);

        public int Ax => extractor.Ax.Extract(code);
        public int Bx => extractor.Bx.Extract(code);
        public int SBx => extractor.SBx.Extract(code);
        public int X => extractor.X.Extract(code);

        #region 5.4BETA 新增
        public int VB => extractor.VB.Extract(code);
        public int VC => extractor.VC.Extract(code);
        #endregion
        public override string ToString()
        {
            return $"{Operand}:{Codepoint}={A},{B},{C},{K},{X}";
        }

    }
}
