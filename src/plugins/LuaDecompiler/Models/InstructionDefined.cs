namespace ZoDream.LuaDecompiler.Models
{
    public enum InstructionType
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

    public class InstructionDefined
    {
        public const int SLOT_FALSE = 30000;  // placeholder slot value for logical false
        public int SLOT_TRUE = 30001;  // placeholder slot value for logical true

        public string Name { get; private set; }
        public InstructionType AType { get; private set; }
        public InstructionType BType { get; private set; }
        public InstructionType CDType { get; private set; }
        public string Description { get; private set; }

        public byte Opcode { get; set; }

        public int ArgsCount {
            get {
                var i = 0;
                if (AType != InstructionType.None)
                {
                    i++;
                }
                if (BType != InstructionType.None)
                {
                    i++;
                }
                if (CDType != InstructionType.None)
                {
                    i++;
                }
                return i;
            }
        }

        public InstructionDefined(string name,
            InstructionType a_type, 
            InstructionType b_type, InstructionType 
            cd_type, string description)
        {
            Name = name;
            AType = a_type;
            BType = b_type;
            CDType = cd_type;
            Description = description;
        }
    }
}
