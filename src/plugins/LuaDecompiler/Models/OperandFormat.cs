namespace ZoDream.LuaDecompiler.Models
{

    public enum OperandFormat
    {
        RAW,
        REGISTER,
        UPVALUE,
        REGISTER_K,
        CONSTANT,
        CONSTANT_INTEGER,
        CONSTANT_STRING,
        FUNCTION,
        IMMEDIATE_INTEGER,
        IMMEDIATE_SIGNED_INTEGER,
        IMMEDIATE_FLOAT,
        JUMP,
        JUMP_NEGATIVE,
    }
}
