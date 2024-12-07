using ZoDream.LuaDecompiler.Attributes;

namespace ZoDream.LuaDecompiler.Models
{
    public enum OperandFormat
    {
        [OperandField(OperandField.A, OperandFieldFormat.RAW)]
        A,
        [OperandField(OperandField.A, OperandFieldFormat.REGISTER)]
        AR,
        [OperandField(OperandField.A, OperandFieldFormat.UPVALUE)]
        AU,
        [OperandField(OperandField.B, OperandFieldFormat.RAW)]
        B,
        [OperandField(OperandField.B, OperandFieldFormat.REGISTER)]
        BR,
        [OperandField(OperandField.B, OperandFieldFormat.REGISTER_K)]
        BRK,
        [OperandField(OperandField.B, OperandFieldFormat.CONSTANT)]
        BK,
        [OperandField(OperandField.B, OperandFieldFormat.CONSTANT_STRING)]
        BKS,
        [OperandField(OperandField.B, OperandFieldFormat.IMMEDIATE_INTEGER)]
        BI,
        [OperandField(OperandField.B, OperandFieldFormat.IMMEDIATE_SIGNED_INTEGER)]
        BsI,
        [OperandField(OperandField.B, OperandFieldFormat.UPVALUE)]
        BU,
        [OperandField(OperandField.C, OperandFieldFormat.RAW)]
        C,
        [OperandField(OperandField.C, OperandFieldFormat.REGISTER)]
        CR,
        [OperandField(OperandField.C, OperandFieldFormat.REGISTER_K)]
        CRK,
        [OperandField(OperandField.C, OperandFieldFormat.REGISTER_K54)]
        CRK54,
        [OperandField(OperandField.C, OperandFieldFormat.CONSTANT)]
        CK,
        [OperandField(OperandField.C, OperandFieldFormat.CONSTANT_INTEGER)]
        CKI,
        [OperandField(OperandField.C, OperandFieldFormat.CONSTANT_STRING)]
        CKS,
        [OperandField(OperandField.C, OperandFieldFormat.IMMEDIATE_INTEGER)]
        CI,
        [OperandField(OperandField.C, OperandFieldFormat.IMMEDIATE_SIGNED_INTEGER)]
        CsI,
        [OperandField(OperandField.k, OperandFieldFormat.RAW)]
        k,
        [OperandField(OperandField.Ax, OperandFieldFormat.RAW)]
        Ax,
        [OperandField(OperandField.sJ, OperandFieldFormat.JUMP)]
        sJ,
        [OperandField(OperandField.Bx, OperandFieldFormat.RAW)]
        Bx,
        [OperandField(OperandField.Bx, OperandFieldFormat.CONSTANT)]
        BxK,
        [OperandField(OperandField.Bx, OperandFieldFormat.JUMP)]
        BxJ,
        [OperandField(OperandField.Bx, OperandFieldFormat.JUMP, 1)]
        BxJ1,
        [OperandField(OperandField.Bx, OperandFieldFormat.JUMP_NEGATIVE)]
        BxJn,
        [OperandField(OperandField.Bx, OperandFieldFormat.FUNCTION)]
        BxF,
        [OperandField(OperandField.sBx, OperandFieldFormat.JUMP)]
        sBxJ,
        [OperandField(OperandField.sBx, OperandFieldFormat.IMMEDIATE_INTEGER)]
        BxI,
        [OperandField(OperandField.sBx, OperandFieldFormat.IMMEDIATE_SIGNED_INTEGER)]
        sBxI,
        [OperandField(OperandField.sBx, OperandFieldFormat.IMMEDIATE_FLOAT)]
        sBxF,
        [OperandField(OperandField.x, OperandFieldFormat.RAW)]
        x,
    }
}
