namespace ZoDream.LuaDecompiler.Models
{
    public enum Operand
    {
        MOVE,
        LOADI,
        LOADF,
        LOADK,
        LOADKX,
        LOADBOOL, // 5.4
        LOADFALSE,
        LFALSESKIP,
        LOADTRUE,
        LOADNIL,

        GETGLOBAL, // 5.1
        SETGLOBAL, // 5.1

        GETUPVAL,
        SETUPVAL,

        GETTABUP,
        GETTABLE,
        GETI,
        GETFIELD,

        SETTABUP,
        SETTABLE,
        SETI,
        SETFIELD,

        NEWTABLE,

        SELF,

        ADDI,

        ADDK,
        SUBK,
        MULK,
        MODK,
        POWK,
        DIVK,
        IDIVK,

        BANDK,
        BORK,
        BXORK,

        SHRI,
        SHLI,

        ADD,
        SUB,
        MUL,
        MOD,
        POW,
        DIV,
        IDIV,

        BAND,
        BOR,
        BXOR,
        SHL,
        SHR,

        MMBIN,
        MMBINI,
        MMBINK,

        UNM,
        BNOT,
        NOT,
        LEN,

        CONCAT,

        CLOSE,
        TBC,
        JMP,
        EQ,
        LT,
        LE,

        EQK,
        EQI,
        LTI,
        LEI,
        GTI,
        GEI,

        TEST,
        TESTSET,

        CALL,
        TAILCALL,

        RETURN,
        RETURN0,
        RETURN1,

        FORLOOP,
        FORPREP,

        TFORPREP,
        TFORCALL,
        TFORLOOP,

        SETLIST,
        SETLISTO, // 5.3

        CLOSURE,

        VARARG,

        VARARGPREP,

        EXTRAARG
    }
}
