using System;
using ZoDream.Shared.Language;

namespace ZoDream.ShaderDecompiler.SpirV
{
    public enum SpvOperandQuantifier
    {
        /// <summary>
        /// 1
        /// </summary>
        Default,
        /// <summary>
        /// 0 or 1
        /// </summary>
        Optional,
        /// <summary>
        /// 0+
        /// </summary>
        Varying
    }
    public class SpvInstructionOperand(OperandType kind, string? name, SpvOperandQuantifier quantifier) : IInstructionOperand
    {
        public string? Name { get; } = name;
        public OperandType Type { get; } = kind;
        public SpvOperandQuantifier Quantifier { get; } = quantifier;
    }
    public class SpvInstruction(SpvOperand code, SpvInstructionOperand[] operands) : IInstruction
    {
        public SpvInstructionOperand[] Operands => operands;

        public SpvOperand Operand => code;

        public string Mnemonic => $"Op{Enum.GetName(code)}";

        public IInstructionOperand[] OperandItems => Operands;
    }
}
