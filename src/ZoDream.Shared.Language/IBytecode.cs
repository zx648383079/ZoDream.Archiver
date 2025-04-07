namespace ZoDream.Shared.Language
{
    public interface IBytecode
    {
        public ILanguageHeader Header { get; }

        public ILanguageChunk MainChunk { get; }
    }

    public interface ILanguageHeader
    {
    }

    public interface ILanguageChunk
    {

        public ILanguageOpcode[] OpcodeItems { get; }
    }

    public interface ILanguageOpcode
    {
    }


}
