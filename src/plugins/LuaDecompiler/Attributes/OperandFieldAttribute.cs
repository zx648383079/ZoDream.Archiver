using System;
using ZoDream.LuaDecompiler.Models;

namespace ZoDream.LuaDecompiler.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OperandFieldAttribute(OperandField field, OperandFieldFormat format) : Attribute
    {
        public OperandFieldAttribute(OperandField field, OperandFieldFormat format, int offset)
            : this (field, format)
        {
            Offset = offset;
        }

        public OperandField Field { get; private set; } = field;
        public OperandFieldFormat Format { get; private set;} = format;
        public int Offset { get; private set;}

    }
}
