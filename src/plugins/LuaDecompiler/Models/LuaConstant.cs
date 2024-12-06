namespace ZoDream.LuaDecompiler.Models
{
    public class LuaConstant
    {
        public LuaConstantType Type { get; set; }

        public object? Value { get; set; }

        public LuaConstant()
        {
            Type = LuaConstantType.Null; 
        }

        public LuaConstant(bool value)
        {
            Type=LuaConstantType.Bool;
            Value = value;
        }

        public LuaConstant(LuaConstantType type, object value)
        {
            Type = type;
            Value = value;
        }

        public LuaConstant(string value)
        {
            Type = LuaConstantType.String;
            Value = value;
        }
    }

    public enum LuaConstantType
    {
        Null,
        Bool,
        Number,
        String,
        // luajit
        Complex,
        Proto,
        Table,
        // luau
        Imp,
    }
}
