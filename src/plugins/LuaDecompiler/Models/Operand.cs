using ZoDream.LuaDecompiler.Attributes;

namespace ZoDream.LuaDecompiler.Models
{
    public enum Operand
    {
        // Lua 5.1 Opcodes
        [Operand("move", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53, LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR)]
        MOVE,
        [Operand("loadk", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53, LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BxK)]
        LOADK,
        [Operand("loadbool", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.B, OperandFormat.C)]
        LOADBOOL,
        [Operand("loadnil", [LuaVersion.Lua50, LuaVersion.Lua51], OperandFormat.AR, OperandFormat.BR)]
        LOADNIL,
        [Operand("getupval", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53, LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BU)]
        GETUPVAL,
        [Operand("getglobal", [LuaVersion.Lua50, LuaVersion.Lua51], OperandFormat.AR, OperandFormat.BxK)]
        GETGLOBAL,
        [Operand("gettable", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.BR, OperandFormat.CRK)]
        GETTABLE,
        [Operand("setglobal", [LuaVersion.Lua50, LuaVersion.Lua51], OperandFormat.AR, OperandFormat.BxK)]
        SETGLOBAL,
        [Operand("setupval", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53, LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BU)]
        SETUPVAL,
        [Operand("settable", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK)]
        SETTABLE,
        [Operand("newtable", [LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.B, OperandFormat.C)]
        NEWTABLE,
        [Operand("self", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.BR, OperandFormat.CRK)]
        SELF,
        [Operand("add", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK)]
        ADD,
        [Operand("sub", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK)]
        SUB,
        [Operand("mul", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK)]
        MUL,
        [Operand("div", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK)]
        DIV,
        [Operand("mod", [LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK)]
        MOD,
        [Operand("pow", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK)]
        POW,
        [Operand("unm", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.BR)]
        UNM,
        [Operand("not", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.BR)]
        NOT,
        [Operand("len", [LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.BR)]
        LEN,
        [Operand("concat", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK)]
        CONCAT,
        [Operand("jmp", [LuaVersion.Lua50, LuaVersion.Lua51], OperandFormat.sBxJ)]
        JMP,
        [Operand("eq", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.A, OperandFormat.BRK, OperandFormat.CRK)]
        EQ,
        [Operand("lt", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.A, OperandFormat.BRK, OperandFormat.CRK)]
        LT,
        [Operand("le", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.A, OperandFormat.BRK, OperandFormat.CRK)]
        LE,
        [Operand("test", [LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.C)]
        TEST,
        [Operand("testset", [LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.BR, OperandFormat.C)]
        TESTSET,
        [Operand("call", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53, LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.B, OperandFormat.C)]
        CALL,
        [Operand("tailcall", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.B)]
        TAILCALL,
        [Operand("return", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.B)]
        RETURN,
        [Operand("forloop", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.sBxJ)]
        FORLOOP,
        [Operand("forprep", [LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.sBxJ)]
        FORPREP,
        [Operand("tforloop", [LuaVersion.Lua50, LuaVersion.Lua51], OperandFormat.AR, OperandFormat.C)]
        TFORLOOP,
        [Operand("setlist", LuaVersion.Lua51, OperandFormat.AR, OperandFormat.B, OperandFormat.C)]
        SETLIST,
        [Operand("close", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR)]
        CLOSE,
        [Operand("closure", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53, LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BxF)]
        CLOSURE,
        [Operand("vararg", [LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.B)]
        VARARG,
        // Lua 5.2 Opcodes
        [Operand("jmp", [LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.A, OperandFormat.sBxJ)]
        JMP52,
        [Operand("loadnil", [LuaVersion.Lua52, LuaVersion.Lua53, LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.B)]
        LOADNIL52,
        [Operand("loadkx", [LuaVersion.Lua52, LuaVersion.Lua53, LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR)]
        LOADKX,
        [Operand("gettabup", [LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.BU, OperandFormat.CRK)]
        GETTABUP,
        [Operand("settabup", [LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AU, OperandFormat.BRK, OperandFormat.CRK)]
        SETTABUP,
        [Operand("setlist", [LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.B, OperandFormat.C)]
        SETLIST52,
        [Operand("tforcall", [LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.C)]
        TFORCALL,
        [Operand("tforloop", [LuaVersion.Lua52, LuaVersion.Lua53], OperandFormat.AR, OperandFormat.sBxJ)]
        TFORLOOP52,
        [Operand("extraarg", [LuaVersion.Lua52, LuaVersion.Lua53, LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.Ax)]
        EXTRAARG,
        // Lua 5.0 Opcodes
        [Operand("newtable", LuaVersion.Lua50, OperandFormat.AR, OperandFormat.B, OperandFormat.C)]
        NEWTABLE50,
        [Operand("setlist", LuaVersion.Lua50, OperandFormat.AR, OperandFormat.Bx)]
        SETLIST50,
        [Operand("setlisto", LuaVersion.Lua50, OperandFormat.AR, OperandFormat.Bx)]
        SETLISTO,
        [Operand("tforprep", LuaVersion.Lua50, OperandFormat.AR, OperandFormat.sBxJ)]
        TFORPREP,
        [Operand("test", LuaVersion.Lua50, OperandFormat.AR, OperandFormat.BR, OperandFormat.C)]
        TEST50,
        // Lua 5.3 Opcodes
        [Operand("idiv", LuaVersion.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK)]
        IDIV,
        [Operand("band", LuaVersion.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK)]
        BAND,
        [Operand("bor", LuaVersion.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK)]
        BOR,
        [Operand("bxor", LuaVersion.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK)]
        BXOR,
        [Operand("shl", LuaVersion.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK)]
        SHL,
        [Operand("shr", LuaVersion.Lua53, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK)]
        SHR,
        [Operand("bnot", [LuaVersion.Lua53, LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR)]
        BNOT,
        // Lua 5.4 Opcodes
        [Operand("loadi", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BxI)]
        LOADI,
        [Operand("loadf", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.sBxF)]
        LOADF,
        [Operand("loadfalse", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR)]
        LOADFALSE,
        [Operand("lfalseskip", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR)]
        LFALSESKIP,
        [Operand("loadtrue", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR)]
        LOADTRUE,
        [Operand("gettabup", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BU, OperandFormat.CKS)]
        GETTABUP54,
        [Operand("gettable", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CR)]
        GETTABLE54,
        [Operand("geti", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CI)]
        GETI,
        [Operand("getfield", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CKS)]
        GETFIELD,
        [Operand("settabup", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AU, OperandFormat.BK, OperandFormat.CRK54)]
        SETTABUP54,
        [Operand("settable", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CRK54)]
        SETTABLE54,
        [Operand("seti", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BI, OperandFormat.CRK54)]
        SETI,
        [Operand("setfield", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BKS, OperandFormat.CRK54)]
        SETFIELD,
        [Operand("newtable", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.B, OperandFormat.C, OperandFormat.k)]
        NEWTABLE54,
        [Operand("self", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CRK54)]
        SELF54,
        [Operand("addi", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CsI)]
        ADDI,
        [Operand("addk", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CK)]
        ADDK,
        [Operand("subk", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CK)]
        SUBK,
        [Operand("mulk", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CK)]
        MULK,
        [Operand("modk", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CK)]
        MODK,
        [Operand("powk", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CK)]
        POWK,
        [Operand("divk", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CK)]
        DIVK,
        [Operand("idivk", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CK)]
        IDIVK,
        [Operand("bandk", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CKI)]
        BANDK,
        [Operand("bork", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CKI)]
        BORK,
        [Operand("bxork", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CKI)]
        BXORK,
        [Operand("shri", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CsI)]
        SHRI,
        [Operand("shli", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.CsI, OperandFormat.BR)]
        SHLI,
        [Operand("add", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CR)]
        ADD54,
        [Operand("sub", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CR)]
        SUB54,
        [Operand("mul", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CR)]
        MUL54,
        [Operand("mod", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CR)]
        MOD54,
        [Operand("pow", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CR)]
        POW54,
        [Operand("div", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CR)]
        DIV54,
        [Operand("idiv", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CR)]
        IDIV54,
        [Operand("band", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CR)]
        BAND54,
        [Operand("bor", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CR)]
        BOR54,
        [Operand("bxor", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CR)]
        BXOR54,
        [Operand("shl", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CR)]
        SHL54,
        [Operand("shr", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.CR)]
        SHR54,
        [Operand("mmbin", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.C)]
        MMBIN,
        [Operand("mmbini", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BsI, OperandFormat.C, OperandFormat.k)]
        MMBINI,
        [Operand("mmbink", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BK, OperandFormat.C, OperandFormat.k)]
        MMBINK,
        [Operand("concat", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.B)]
        CONCAT54,
        [Operand("tbc", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR)]
        TBC,
        [Operand("jmp", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.sJ)]
        JMP54,
        [Operand("eq", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.k)]
        EQ54,
        [Operand("lt", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.k)]
        LT54,
        [Operand("le", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.k)]
        LE54,
        [Operand("eqk", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BK, OperandFormat.k)]
        EQK,
        [Operand("eqi", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BsI, OperandFormat.k, OperandFormat.C)]
        EQI,
        [Operand("lti", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BsI, OperandFormat.k, OperandFormat.C)]
        LTI,
        [Operand("lei", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BsI, OperandFormat.k, OperandFormat.C)]
        LEI,
        [Operand("gti", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BsI, OperandFormat.k, OperandFormat.C)]
        GTI,
        [Operand("gei", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BsI, OperandFormat.k, OperandFormat.C)]
        GEI,
        [Operand("test", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.k)]
        TEST54,
        [Operand("testset", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BR, OperandFormat.k)]
        TESTSET54,
        [Operand("tailcall", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.B, OperandFormat.C, OperandFormat.k)]
        TAILCALL54,
        [Operand("return", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.B, OperandFormat.C, OperandFormat.k)]
        RETURN54,
        [Operand("return0", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.B, OperandFormat.C, OperandFormat.k)]
        RETURN0,
        [Operand("return1", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.B, OperandFormat.C, OperandFormat.k)]
        RETURN1,
        [Operand("forloop", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BxJn)]
        FORLOOP54,
        [Operand("forprep", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BxJ)]
        FORPREP54,
        [Operand("tforprep", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BxJ)]
        TFORPREP54,
        [Operand("tforcall", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.C)]
        TFORCALL54,
        [Operand("tforloop", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.BxJn)]
        TFORLOOP54,
        [Operand("setlist", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.B, OperandFormat.C, OperandFormat.k)]
        SETLIST54,
        [Operand("vararg", [LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.AR, OperandFormat.C)]
        VARARG54,
        [Operand("varargprep", LuaVersion.Lua54, OperandFormat.A)]
        VARARGPREP,
        // Special
        [Operand("extrabyte", [LuaVersion.Lua50, LuaVersion.Lua51, LuaVersion.Lua52, LuaVersion.Lua53, LuaVersion.Lua54, LuaVersion.Lua54Beta], OperandFormat.x)]
        EXTRABYTE,
        [Operand("default", 0, OperandFormat.AR, OperandFormat.BRK, OperandFormat.CRK)]
        DEFAULT,
        [Operand("default", 0, OperandFormat.AR, OperandFormat.BR, OperandFormat.C, OperandFormat.k)]
        DEFAULT54,

    }
}
