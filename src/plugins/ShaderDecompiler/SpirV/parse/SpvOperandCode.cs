using ZoDream.Shared.Language;

namespace ZoDream.ShaderDecompiler.SpirV
{
    public class SpvOperandCode(SpvOperand op, uint[] data) : ILanguageOpcode
    {
        public SpvOperand Operand { get; set; } = op;

        public uint[] Data { get; set; } = data;
    }
}
