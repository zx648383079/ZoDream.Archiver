using ZoDream.Shared.Language;

namespace ZoDream.ShaderDecompiler.SpirV
{
    public class SpvChunk : ILanguageChunk
    {
        public SpvOperandCode[] OpcodeItems { get; internal set; } = [];

        ILanguageOpcode[] ILanguageChunk.OpcodeItems => OpcodeItems;

    }
}
