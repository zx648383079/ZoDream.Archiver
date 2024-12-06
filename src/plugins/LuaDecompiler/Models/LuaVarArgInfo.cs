namespace ZoDream.LuaDecompiler.Models
{
    public class LuaVarArgInfo
    {
        public bool HasArg { get; set; }
        public bool NeedArg { get; set; }

        public LuaVarArgInfo()
        {
            
        }

        public LuaVarArgInfo(byte code)
        {
            HasArg = (code & 1) != 0;
            NeedArg = (code & 4) != 0;
        }
    }
}
