namespace ZoDream.LuaDecompiler.Models
{
    public class LuaBytecode
    {
        public LuaHeader Header { get; set; } = new();

        public LuaChunk MainChunk { get; set; } = new();
    }
}
