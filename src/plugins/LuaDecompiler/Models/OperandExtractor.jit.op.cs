using System;
using System.Reflection;
using ZoDream.LuaDecompiler.Attributes;

namespace ZoDream.LuaDecompiler.Models
{
    public partial class JitOperandExtractor
    {
        #region op

        public static JitOperand ParseOperand(byte code, LuaHeader header)
        {
            return ParseOperand(code, header.Version);
        }
        public static JitOperand ParseOperand(byte code, LuaVersion version)
        {
            if (version == LuaVersion.LuaJit1)
            {
                throw new NotSupportedException();
            }
            if (version == LuaVersion.LuaJit2)
            {
                if (code < (byte)JitOperand.ISTYPE)
                {
                    return (JitOperand)code;
                }
                if (code < (byte)JitOperand.TGETR - 2)
                {
                    return (JitOperand)(code + 2);
                }
                if (code < (byte)JitOperand.TSETR - 3)
                {
                    return (JitOperand)(code + 3);
                }
                return (JitOperand)(code + 4);
            }
            return (JitOperand)code;
        }

        public static bool IsABCFormat(JitOperand operand)
        {
            return GetAttribute(operand)?.ArgsCount == 3;
        }

        public static JitOperandAttribute? GetAttribute(JitOperand operand)
        {
            var type = operand.GetType();
            var name = Enum.GetName(operand);
            if (name is null)
            {
                return null;
            }
            var field = type.GetField(name);
            return field?.GetCustomAttribute<JitOperandAttribute>();
        }
        #endregion

    }
}
