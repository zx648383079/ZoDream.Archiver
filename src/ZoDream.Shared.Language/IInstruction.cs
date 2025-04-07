namespace ZoDream.Shared.Language
{
    public interface IInstruction
    {

        public IInstructionOperand[] OperandItems { get; }

        public string Mnemonic { get; }
    }

    public interface IInstructionOperand
    {
    }
}
