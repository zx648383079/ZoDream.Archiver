namespace ZoDream.LuaDecompiler.Models
{
    public class LuaLocalVar
    {
        public string Name { get; set; } = string.Empty;
        public ulong StartPc { get; set; }
        public ulong EndPc { get; set; }

        public override string ToString()
        {
            return $"[{StartPc}-{EndPc}]{Name}";
        }
    }
}
