using ZoDream.Shared.Language;

namespace ZoDream.LuaDecompiler.Models
{
    public class LuaBytecode : IBytecode
    {
        public LuaHeader Header { get; set; } = new();

        public LuaChunk MainChunk { get; set; } = new();

        ILanguageHeader IBytecode.Header => Header;

        ILanguageChunk IBytecode.MainChunk => MainChunk;
    }
}
