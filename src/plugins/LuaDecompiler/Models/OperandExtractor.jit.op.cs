using System;
using System.Reflection;
using ZoDream.LuaDecompiler.Attributes;

namespace ZoDream.LuaDecompiler.Models
{
    public partial class JitOperandExtractor
    {
        #region op

        

        private static readonly JitOperand[] _opcodeMapV20 = [
            JitOperand.ISLT,
            JitOperand.ISGE,
            JitOperand.ISLE,
            JitOperand.ISGT,

            JitOperand.ISEQV,
            JitOperand.ISNEV,

            JitOperand.ISEQS,
            JitOperand.ISNES,

            JitOperand.ISEQN,
            JitOperand.ISNEN,

            JitOperand.ISEQP,
            JitOperand.ISNEP,

            // Unary test and copy ops

            JitOperand.ISTC,
            JitOperand.ISFC,

            JitOperand.IST,
            JitOperand.ISF,

            // Unary ops

            JitOperand.MOV,
            JitOperand.NOT,
            JitOperand.UNM,
            JitOperand.LEN,

            // Binary ops

            JitOperand.ADDVN,
            JitOperand.SUBVN,
            JitOperand.MULVN,
            JitOperand.DIVVN,
            JitOperand.MODVN,

            JitOperand.ADDNV,
            JitOperand.SUBNV,
            JitOperand.MULNV,
            JitOperand.DIVNV,
            JitOperand.MODNV,

            JitOperand.ADDVV,
            JitOperand.SUBVV,
            JitOperand.MULVV,
            JitOperand.DIVVV,
            JitOperand.MODVV,

            JitOperand.POW,
            JitOperand.CAT,

            // Constant ops

            JitOperand.KSTR,
            JitOperand.KCDATA,
            JitOperand.KSHORT,
            JitOperand.KNUM,
            JitOperand.KPRI,

            JitOperand.KNIL,

            // Upvalue and function ops

            JitOperand.UGET,

            JitOperand.USETV,
            JitOperand.USETS,
            JitOperand.USETN,
            JitOperand.USETP,

            JitOperand.UCLO,

            JitOperand.FNEW,
            // Table ops

            JitOperand.TNEW,

            JitOperand.TDUP,

            JitOperand.GGET,
            JitOperand.GSET,

            JitOperand.TGETV,
            JitOperand.TGETS,
            JitOperand.TGETB,

            JitOperand.TSETV,
            JitOperand.TSETS,
            JitOperand.TSETB,

            JitOperand.TSETM,

            // Calls and vararg handling

            JitOperand.CALLM,
            JitOperand.CALL,
            JitOperand.CALLMT,
            JitOperand.CALLT,

            JitOperand.ITERC,
            JitOperand.ITERN,

            JitOperand.VARG,

            JitOperand.ISNEXT,

            // Returns

            JitOperand.RETM,
            JitOperand.RET,
            JitOperand.RET0,
            JitOperand.RET1,

            // Loops and branches

            JitOperand.FORI,
            JitOperand.JFORI,

            JitOperand.FORL,
            JitOperand.IFORL,
            JitOperand.JFORL,

            JitOperand.ITERL,
            JitOperand.IITERL,
            JitOperand.JITERL,

            JitOperand.LOOP,
            JitOperand.ILOOP,
            JitOperand.JLOOP,

            JitOperand.JMP,

            // Function headers

            JitOperand.FUNCF,
            JitOperand.IFUNCF,
            JitOperand.JFUNCF,

            JitOperand.FUNCV,
            JitOperand.IFUNCV,
            JitOperand.JFUNCV,

            JitOperand.FUNCC,
            JitOperand.FUNCCW,];

        private static readonly JitOperand[] _opcodeMapV21 = [
            JitOperand.ISLT,
            JitOperand.ISGE,
            JitOperand.ISLE,
            JitOperand.ISGT,

            JitOperand.ISEQV,
            JitOperand.ISNEV,

            JitOperand.ISEQS,
            JitOperand.ISNES,

            JitOperand.ISEQN,
            JitOperand.ISNEN,

            JitOperand.ISEQP,
            JitOperand.ISNEP,

            // Unary test and copy ops

            JitOperand.ISTC,
            JitOperand.ISFC,

            JitOperand.IST,
            JitOperand.ISF,
            JitOperand.ISTYPE,
            JitOperand.ISNUM,

            // Unary ops

            JitOperand.MOV,
            JitOperand.NOT,
            JitOperand.UNM,
            JitOperand.LEN,

            // Binary ops

            JitOperand.ADDVN,
            JitOperand.SUBVN,
            JitOperand.MULVN,
            JitOperand.DIVVN,
            JitOperand.MODVN,

            JitOperand.ADDNV,
            JitOperand.SUBNV,
            JitOperand.MULNV,
            JitOperand.DIVNV,
            JitOperand.MODNV,

            JitOperand.ADDVV,
            JitOperand.SUBVV,
            JitOperand.MULVV,
            JitOperand.DIVVV,
            JitOperand.MODVV,

            JitOperand.POW,
            JitOperand.CAT,

            // Constant ops

            JitOperand.KSTR,
            JitOperand.KCDATA,
            JitOperand.KSHORT,
            JitOperand.KNUM,
            JitOperand.KPRI,

            JitOperand.KNIL,

            // Upvalue and function ops

            JitOperand.UGET,

            JitOperand.USETV,
            JitOperand.USETS,
            JitOperand.USETN,
            JitOperand.USETP,

            JitOperand.UCLO,

            JitOperand.FNEW,

            // Table ops

            JitOperand.TNEW,

            JitOperand.TDUP,

            JitOperand.GGET,
            JitOperand.GSET,

            JitOperand.TGETV,
            JitOperand.TGETS,
            JitOperand.TGETB,
            JitOperand.TGETR,

            JitOperand.TSETV,
            JitOperand.TSETS,
            JitOperand.TSETB,

            JitOperand.TSETM,
            JitOperand.TSETR,

            // Calls and vararg handling

            JitOperand.CALLM,
            JitOperand.CALL,
            JitOperand.CALLMT,
            JitOperand.CALLT,

            JitOperand.ITERC,
            JitOperand.ITERN,

            JitOperand.VARG,

            JitOperand.ISNEXT,

            // Returns

            JitOperand.RETM,
            JitOperand.RET,
            JitOperand.RET0,
            JitOperand.RET1,

            // Loops and branches

            JitOperand.FORI,
            JitOperand.JFORI,

            JitOperand.FORL,
            JitOperand.IFORL,
            JitOperand.JFORL,

            JitOperand.ITERL,
            JitOperand.IITERL,
            JitOperand.JITERL,

            JitOperand.LOOP,
            JitOperand.ILOOP,
            JitOperand.JLOOP,

            JitOperand.JMP,

            // Function headers

            JitOperand.FUNCF,
            JitOperand.IFUNCF,
            JitOperand.JFUNCF,

            JitOperand.FUNCV,
            JitOperand.IFUNCV,
            JitOperand.JFUNCV,

            JitOperand.FUNCC,
            JitOperand.FUNCCW
        ];


        public static JitOperand ParseOperand(byte code, LuaHeader header)
        {
            return ParseOperand(code, header.Version);
        }
        public static JitOperand ParseOperand(byte code, LuaVersion version)
        {
            if (version == LuaVersion.LuaJit1)
            {
                if (code < (byte)JitOperand.ISTYPE)
                {
                    return (JitOperand)code;
                }
                if (code < (byte)JitOperand.TGETR - 2)
                {
                    return (JitOperand)(code + 2);
                }
                if (code >= (byte)JitOperand.TSETR - 3)
                {
                    return (JitOperand)(code + 4);
                }
                return (JitOperand)(code + 3);
            }
            if (version == LuaVersion.LuaJit2)
            {
                return _opcodeMapV20[code];
            }
            return _opcodeMapV21[code];
        }

        public static bool IsABCFormat(JitOperand operand)
        {
            var type = operand.GetType();
            var name = Enum.GetName(operand);
            if (name is null)
            {
                return false;
            }
            var field = type.GetField(name);
            var attr = field?.GetCustomAttribute<JitOperandAttribute>();
            return attr?.ArgsCount == 3;
        }
        #endregion
        
    }
}
