namespace ZoDream.LuaDecompiler.Models
{
    public class LuaDebugInfo
    {
        public uint[] LineNoItems { get; internal set; } = [];

        public LuaLocalVar[] LocalItems { get; internal set; } = [];
        public LuaLineInfo[] AbsoluteLineItems { get; internal set; } = [];

        public LuaUpValue[] UpValueItems { get; internal set; } = [];
        public string[] UpValueNameItems { get; internal set; } = [];
    }
}
