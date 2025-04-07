using ZoDream.Shared.Language;

namespace ZoDream.ShaderDecompiler.SpirV
{
    public class SpvBytecode : IBytecode
    {
        public SpvHeader Header { get; internal set; } = new();
        
        public SpvChunk MainChunk { get; internal set; } = new();

        ILanguageHeader IBytecode.Header => Header;

        ILanguageChunk IBytecode.MainChunk => MainChunk;
    }
}
