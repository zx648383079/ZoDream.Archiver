namespace ZoDream.LuaDecompiler.Models
{
    public enum JitOperandFormat
    {
        None,
        VAR, // variable slot number
        DST,  // variable slot number, used as a destination
        BS,  // base slot number, read-write
        RBS,  // base slot number, read-only
        UV,  // upvalue number (slot number, but specific to upvalues)
        LIT, // literal
        SLIT,  // signed literal
        PRI,  // primitive type (0 = nil, 1 = false, 2 = true)
        NUM,  // numeric constant, index into constant table
        STR,  // string constant, negated index into constant table
        TAB,  // template table, negated index into constant table
        FUN,  // function prototype, negated index into constant table
        CDT,  // cdata constant, negated index into constant table
        JMP,  // branch target, relative to next instruction, biased with 0x8000
    }
}
