namespace ZoDream.LuaDecompiler.Models
{
    public enum LuaVersion
    {
        Lua50 = 0x50,
        Lua51 = 0x51,
        Lua52 = 0x52,
        Lua53 = 0x53,
        Lua54 = 0x54,
        Lua54Beta = 0x55, // 无法自动判断
        LuaJit1 = 0x1,
        LuaJit2 = 0x2,
        LuaJit21 = 0x3, // 无法自动判断 
    }
}
