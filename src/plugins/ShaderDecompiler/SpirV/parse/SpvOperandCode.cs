namespace ZoDream.ShaderDecompiler.SpirV
{
    public class SpvOperandCode(SpvOperand op, uint[] data)
    {
        public SpvOperand Operand { get; set; } = op;

        public uint[] Data { get; set; } = data;
    }
}
