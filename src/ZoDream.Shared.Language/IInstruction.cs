namespace ZoDream.Shared.Language
{
    public interface IInstruction
    {

        public IOperand[] Operands { get; }

        public string Mnemonic { get; }
    }

    public interface IOperand
    {
    }
}
