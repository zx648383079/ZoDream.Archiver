namespace ZoDream.LuaDecompiler.Models
{
    public class LuaLineInfo(ulong pc, uint line)
    {
        public ulong ProgramCounter { get; set; } = pc;

        public uint Line { get; set; } = line;

        public override string ToString()
        {
            return $"[{Line}]{ProgramCounter}";
        }
    }
}
