using System;
using ZoDream.LuaDecompiler.Models;

namespace ZoDream.LuaDecompiler.Attributes
{
    [AttributeUsage(AttributeTargets.Field)]
    public class JitOperandAttribute : Attribute
    {
        public const int SLOT_FALSE = 30000;  // placeholder slot value for logical false
        public int SLOT_TRUE = 30001;  // placeholder slot value for logical true

        
        public string Name { get; private set; }
        public JitOperandFormat AType { get; private set; }
        public JitOperandFormat BType { get; private set; }
        public JitOperandFormat CDType { get; private set; }
        public int ArgsCount {
            get {
                var i = 0;
                if (AType != JitOperandFormat.None)
                {
                    i++;
                }
                if (BType != JitOperandFormat.None)
                {
                    i++;
                }
                if (CDType != JitOperandFormat.None)
                {
                    i++;
                }
                return i;
            }
        }

        public JitOperandAttribute(string name,
            JitOperandFormat a_type,
            JitOperandFormat b_type, JitOperandFormat
            cd_type)
        {
            Name = name;
            AType = a_type;
            BType = b_type;
            CDType = cd_type;
        }
    }
}
