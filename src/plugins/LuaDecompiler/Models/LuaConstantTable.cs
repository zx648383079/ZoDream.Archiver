namespace ZoDream.LuaDecompiler.Models
{
    public class LuaConstantTable
    {
        public LuaConstant[] Items { get; set; } = [];
        public (LuaConstant, LuaConstant)[] HashItems { get; set; } = [];
    }
}
