using ZoDream.LuaDecompiler.Models;
using ZoDream.Shared.Language;

namespace ZoDream.LuaDecompiler
{
    public partial class LuaWriter
    {
        private void Translate(ICodeWriter builder, LuaChunk chunk, int index, OperandCode code)
        {
            var attr = OperandExtractor.GetAttribute(code.Operand);
            if (attr is null)
            {
                return;
            }
            switch (code.Operand)
            {
                case Operand.MOVE:
                    break;
                case Operand.LOADK:
                    break;
                case Operand.LOADBOOL:
                    break;
                case Operand.LOADNIL:
                    break;
                case Operand.GETUPVAL:
                    break;
                case Operand.GETGLOBAL:
                    builder.Add(GlobalExpression.Identifier((string)chunk.ConstantItems[code.Bx].Value));
                    break;
                case Operand.GETTABLE:
                    break;
                case Operand.SETGLOBAL:
                    break;
                case Operand.SETUPVAL:
                    break;
                case Operand.SETTABLE:
                    break;
                case Operand.NEWTABLE:
                    break;
                case Operand.SELF:
                    break;
                case Operand.ADD:
                    break;
                case Operand.SUB:
                    break;
                case Operand.MUL:
                    break;
                case Operand.DIV:
                    break;
                case Operand.MOD:
                    break;
                case Operand.POW:
                    break;
                case Operand.UNM:
                    break;
                case Operand.NOT:
                    break;
                case Operand.LEN:
                    break;
                case Operand.CONCAT:
                    break;
                case Operand.JMP:
                    break;
                case Operand.EQ:
                    break;
                case Operand.LT:
                    break;
                case Operand.LE:
                    break;
                case Operand.TEST:
                    break;
                case Operand.TESTSET:
                    break;
                case Operand.CALL:
                    break;
                case Operand.TAILCALL:
                    break;
                case Operand.RETURN:
                    break;
                case Operand.FORLOOP:
                    break;
                case Operand.FORPREP:
                    break;
                case Operand.TFORLOOP:
                    break;
                case Operand.SETLIST:
                    break;
                case Operand.CLOSE:
                    break;
                case Operand.CLOSURE:
                    break;
                case Operand.VARARG:
                    break;
                case Operand.JMP52:
                    break;
                case Operand.LOADNIL52:
                    break;
                case Operand.LOADKX:
                    break;
                case Operand.GETTABUP:
                    break;
                case Operand.SETTABUP:
                    break;
                case Operand.SETLIST52:
                    break;
                case Operand.TFORCALL:
                    break;
                case Operand.TFORLOOP52:
                    break;
                case Operand.EXTRAARG:
                    break;
                case Operand.NEWTABLE50:
                    break;
                case Operand.SETLIST50:
                    break;
                case Operand.SETLISTO:
                    break;
                case Operand.TFORPREP:
                    break;
                case Operand.TEST50:
                    break;
                case Operand.IDIV:
                    break;
                case Operand.BAND:
                    break;
                case Operand.BOR:
                    break;
                case Operand.BXOR:
                    break;
                case Operand.SHL:
                    break;
                case Operand.SHR:
                    break;
                case Operand.BNOT:
                    break;
                case Operand.LOADI:
                    break;
                case Operand.LOADF:
                    break;
                case Operand.LOADFALSE:
                    break;
                case Operand.LFALSESKIP:
                    break;
                case Operand.LOADTRUE:
                    break;
                case Operand.GETTABUP54:
                    break;
                case Operand.GETTABLE54:
                    break;
                case Operand.GETI:
                    break;
                case Operand.GETFIELD:
                    break;
                case Operand.SETTABUP54:
                    break;
                case Operand.SETTABLE54:
                    break;
                case Operand.SETI:
                    break;
                case Operand.SETFIELD:
                    break;
                case Operand.NEWTABLE54:
                    break;
                case Operand.SELF54:
                    break;
                case Operand.ADDI:
                    break;
                case Operand.ADDK:
                    break;
                case Operand.SUBK:
                    break;
                case Operand.MULK:
                    break;
                case Operand.MODK:
                    break;
                case Operand.POWK:
                    break;
                case Operand.DIVK:
                    break;
                case Operand.IDIVK:
                    break;
                case Operand.BANDK:
                    break;
                case Operand.BORK:
                    break;
                case Operand.BXORK:
                    break;
                case Operand.SHRI:
                    break;
                case Operand.SHLI:
                    break;
                case Operand.ADD54:
                    break;
                case Operand.SUB54:
                    break;
                case Operand.MUL54:
                    break;
                case Operand.MOD54:
                    break;
                case Operand.POW54:
                    break;
                case Operand.DIV54:
                    break;
                case Operand.IDIV54:
                    break;
                case Operand.BAND54:
                    break;
                case Operand.BOR54:
                    break;
                case Operand.BXOR54:
                    break;
                case Operand.SHL54:
                    break;
                case Operand.SHR54:
                    break;
                case Operand.MMBIN:
                    break;
                case Operand.MMBINI:
                    break;
                case Operand.MMBINK:
                    break;
                case Operand.CONCAT54:
                    break;
                case Operand.TBC:
                    break;
                case Operand.JMP54:
                    break;
                case Operand.EQ54:
                    break;
                case Operand.LT54:
                    break;
                case Operand.LE54:
                    break;
                case Operand.EQK:
                    break;
                case Operand.EQI:
                    break;
                case Operand.LTI:
                    break;
                case Operand.LEI:
                    break;
                case Operand.GTI:
                    break;
                case Operand.GEI:
                    break;
                case Operand.TEST54:
                    break;
                case Operand.TESTSET54:
                    break;
                case Operand.TAILCALL54:
                    break;
                case Operand.RETURN54:
                    break;
                case Operand.RETURN0:
                    break;
                case Operand.RETURN1:
                    break;
                case Operand.FORLOOP54:
                    break;
                case Operand.FORPREP54:
                    break;
                case Operand.TFORPREP54:
                    break;
                case Operand.TFORCALL54:
                    break;
                case Operand.TFORLOOP54:
                    break;
                case Operand.SETLIST54:
                    break;
                case Operand.VARARG54:
                    break;
                case Operand.VARARGPREP:
                    break;
                case Operand.EXTRABYTE:
                    break;
                case Operand.DEFAULT:
                    break;
                case Operand.DEFAULT54:
                    break;
            }
        }
    }
}
