namespace ZoDream.ShaderDecompiler.SpirV
{
    public class SpvBytecode
    {
        public SpvHeader Header { get; internal set; } = new();
        
        public SpvChunk MainChunk { get; internal set; } = new();
    }
}
