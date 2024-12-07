using System;
using ZoDream.LuaDecompiler.Models;

namespace ZoDream.LuaDecompiler.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class OperandAttribute(string name, LuaVersion[] versions, params OperandFormat[] operands) : Attribute
    {
        public OperandAttribute(string name, LuaVersion version, params OperandFormat[] operands)
            : this (name, [version], operands)
        {
            
        }

        public string Name { get; private set; } = name;
        public LuaVersion[] Versions { get; private set; } = versions;
        public OperandFormat[] Operands { get; private set; } = operands;

    }
}
