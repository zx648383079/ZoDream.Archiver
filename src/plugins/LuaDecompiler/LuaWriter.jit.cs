using System;
using ZoDream.LuaDecompiler.Models;

namespace ZoDream.LuaDecompiler
{
    public partial class LuaWriter
    {
        private static Lazy<InstructionDefined[]> _instructionItems => new Lazy<InstructionDefined[]>(() => [
            new("ISLT", InstructionType.VAR, InstructionType.None, InstructionType.VAR, "if {A} < {D}"),
            new("ISGE", InstructionType.VAR, InstructionType.None, InstructionType.VAR, "if {A} >= {D}"),
            new("ISLE", InstructionType.VAR, InstructionType.None, InstructionType.VAR, "if {A} <= {D}"),
            new("ISGT", InstructionType.VAR, InstructionType.None, InstructionType.VAR, "if {A} > {D}"),

            new("ISEQV", InstructionType.VAR, InstructionType.None, InstructionType.VAR, "if {A} == {D}"),
            new("ISNEV", InstructionType.VAR, InstructionType.None, InstructionType.VAR, "if {A} ~= {D}"),

            new("ISEQS", InstructionType.VAR, InstructionType.None, InstructionType.STR, "if {A} == {D}"),
            new("ISNES", InstructionType.VAR, InstructionType.None, InstructionType.STR, "if {A} ~= {D}"),

            new("ISEQN", InstructionType.VAR, InstructionType.None, InstructionType.NUM, "if {A} == {D}"),
            new("ISNEN", InstructionType.VAR, InstructionType.None, InstructionType.NUM, "if {A} ~= {D}"),

            new("ISEQP", InstructionType.VAR, InstructionType.None, InstructionType.PRI, "if {A} == {D}"),
            new("ISNEP", InstructionType.VAR, InstructionType.None, InstructionType.PRI, "if {A} ~= {D}"),

            // Unary test and copy ops

            new("ISTC", InstructionType.DST, InstructionType.None, InstructionType.VAR, "{A} = {D}; if {D}"),
            new("ISFC", InstructionType.DST, InstructionType.None, InstructionType.VAR, "{A} = {D}; if not {D}"),

            new("IST", InstructionType.None, InstructionType.None, InstructionType.VAR, "if {D}"),
            new("ISF", InstructionType.None, InstructionType.None, InstructionType.VAR, "if not {D}"),

            // Added in bytecode version 2
            new("ISTYPE", InstructionType.VAR, InstructionType.None, InstructionType.LIT, "see lj vm source"),
            new("ISNUM", InstructionType.VAR, InstructionType.None, InstructionType.LIT, "see lj vm source"),

            // Unary ops

            new("MOV", InstructionType.DST, InstructionType.None, InstructionType.VAR, "{A} = {D}"),
            new("NOT", InstructionType.DST, InstructionType.None, InstructionType.VAR, "{A} = not {D}"),
            new("UNM", InstructionType.DST, InstructionType.None, InstructionType.VAR, "{A} = -{D}"),
            new("LEN", InstructionType.DST, InstructionType.None, InstructionType.VAR, "{A} = #{D}"),

            // Binary ops

            new("ADDVN", InstructionType.DST, InstructionType.VAR, InstructionType.NUM, "{A} = {B} + {C}"),
            new("SUBVN", InstructionType.DST, InstructionType.VAR, InstructionType.NUM, "{A} = {B} - {C}"),
            new("MULVN", InstructionType.DST, InstructionType.VAR, InstructionType.NUM, "{A} = {B} * {C}"),
            new("DIVVN", InstructionType.DST, InstructionType.VAR, InstructionType.NUM, "{A} = {B} / {C}"),
            new("MODVN", InstructionType.DST, InstructionType.VAR, InstructionType.NUM, "{A} = {B} % {C}"),

            new("ADDNV", InstructionType.DST, InstructionType.VAR, InstructionType.NUM, "{A} = {C} + {B}"),
            new("SUBNV", InstructionType.DST, InstructionType.VAR, InstructionType.NUM, "{A} = {C} - {B}"),
            new("MULNV", InstructionType.DST, InstructionType.VAR, InstructionType.NUM, "{A} = {C} * {B}"),
            new("DIVNV", InstructionType.DST, InstructionType.VAR, InstructionType.NUM, "{A} = {C} / {B}"),
            new("MODNV", InstructionType.DST, InstructionType.VAR, InstructionType.NUM, "{A} = {C} % {B}"),

            new("ADDVV", InstructionType.DST, InstructionType.VAR, InstructionType.VAR, "{A} = {B} + {C}"),
            new("SUBVV", InstructionType.DST, InstructionType.VAR, InstructionType.VAR, "{A} = {B} - {C}"),
            new("MULVV", InstructionType.DST, InstructionType.VAR, InstructionType.VAR, "{A} = {B} * {C}"),
            new("DIVVV", InstructionType.DST, InstructionType.VAR, InstructionType.VAR, "{A} = {B} / {C}"),
            new("MODVV", InstructionType.DST, InstructionType.VAR, InstructionType.VAR, "{A} = {B} % {C}"),

            new("POW", InstructionType.DST, InstructionType.VAR, InstructionType.VAR, "{A} = {B} ^ {C} (pow)"),
            new("CAT", InstructionType.DST, InstructionType.RBS, InstructionType.RBS,
                        "{A} = {concaInstructionType.from_B_to_C}"),

            // Constant ops.

            new("KSTR", InstructionType.DST, InstructionType.None, InstructionType.STR, "{A} = {D}"),
            new("KCDATA", InstructionType.DST, InstructionType.None, InstructionType.CDT, "{A} = {D}"),
            new("KSHORT", InstructionType.DST, InstructionType.None, InstructionType.SLIT, "{A} = {D}"),
            new("KNUM", InstructionType.DST, InstructionType.None, InstructionType.NUM, "{A} = {D}"),
            new("KPRI", InstructionType.DST, InstructionType.None, InstructionType.PRI, "{A} = {D}"),

            new("KNIL", InstructionType.BS, InstructionType.None, InstructionType.BS, "{from_A_to_D} = nil"),

            // Upvalue and function ops.

            new("UGET", InstructionType.DST, InstructionType.None, InstructionType.UV, "{A} = {D}"),

            new("USETV", InstructionType.UV, InstructionType.None, InstructionType.VAR, "{A} = {D}"),
            new("USETS", InstructionType.UV, InstructionType.None, InstructionType.STR, "{A} = {D}"),
            new("USETN", InstructionType.UV, InstructionType.None, InstructionType.NUM, "{A} = {D}"),
            new("USETP", InstructionType.UV, InstructionType.None, InstructionType.PRI, "{A} = {D}"),

            new("UCLO", InstructionType.RBS, InstructionType.None, InstructionType.JMP,
                         "nil uvs >= {A}; goto {D}"),

            new("FNEW", InstructionType.DST, InstructionType.None, InstructionType.FUN, "{A} = function {D}"),

            // Table ops.

            new("TNEW", InstructionType.DST, InstructionType.None, InstructionType.LIT, "{A} = new table(\n array: {D_array},\n dict: {D_dict})"),

            new("TDUP", InstructionType.DST, InstructionType.None, InstructionType.TAB, "{A} = copy {D}"),

            new("GGET", InstructionType.DST, InstructionType.None, InstructionType.STR, "{A} = _env[{D}]"),
            new("GSET", InstructionType.VAR, InstructionType.None, InstructionType.STR, "_env[{D}] = {A}"),

            new("TGETV", InstructionType.DST, InstructionType.VAR, InstructionType.VAR, "{A} = {B}[{C}]"),
            new("TGETS", InstructionType.DST, InstructionType.VAR, InstructionType.STR, "{A} = {B}.{C}"),
            new("TGETB", InstructionType.DST, InstructionType.VAR, InstructionType.LIT, "{A} = {B}[{C}]"),

            // Added in bytecode version 2
            new("TGETR", InstructionType.DST, InstructionType.VAR, InstructionType.VAR, "{A} = {B}[{C}]"),

            new("TSETV", InstructionType.VAR, InstructionType.VAR, InstructionType.VAR, "{B}[{C}] = {A}"),
            new("TSETS", InstructionType.VAR, InstructionType.VAR, InstructionType.STR, "{B}.{C} = {A}"),
            new("TSETB", InstructionType.VAR, InstructionType.VAR, InstructionType.LIT, "{B}[{C}] = {A}"),

            new("TSETM", InstructionType.BS, InstructionType.None, InstructionType.NUM,
                          "for i = 0, MULTRES, 1 do\n {A_minus_one}[{D_low} + i] = slot({A} + i)"),

            // Added in bytecode version 2
            new("TSETR", InstructionType.VAR, InstructionType.VAR, InstructionType.VAR, "{B}[{C}] = {A}"),

            // Calls and vararg handling. T = tail call.

            new("CALLM", InstructionType.BS, InstructionType.LIT, InstructionType.LIT,
                          "{from_A_x_B_minus_two} = {A}({from_A_plus_one_x_C}, ...MULTRES)"),

            new("CALL", InstructionType.BS, InstructionType.LIT, InstructionType.LIT,
                         "{from_A_x_B_minus_two} = {A}({from_A_plus_one_x_C_minus_one})"),

            new("CALLMT", InstructionType.BS, InstructionType.None, InstructionType.LIT,
                           "return {A}({from_A_plus_one_x_D}, ...MULTRES)"),

            new("CALLT", InstructionType.BS, InstructionType.None, InstructionType.LIT,
                          "return {A}({from_A_plus_one_x_D_minus_one})"),

            new("ITERC", InstructionType.BS, InstructionType.LIT, InstructionType.LIT,
                          "{A}, {A_plus_one}, {A_plus_two} =\n {A_minus_three}, {A_minus_two}, {A_minus_one};\n {from_A_x_B_minus_two} =\n {A_minus_three}({A_minus_two}, {A_minus_one})"),

            new("ITERN", InstructionType.BS, InstructionType.LIT, InstructionType.LIT,
                          "{A}, {A_plus_one}, {A_plus_two} =\n {A_minus_three}, {A_minus_two}, {A_minus_one};\n {from_A_x_B_minus_two} =\n {A_minus_three}({A_minus_two}, {A_minus_one})"),

            new("VARG", InstructionType.BS, InstructionType.LIT, InstructionType.LIT,
                         "{from_A_x_B_minus_two} = ..."),

            new("ISNEXT", InstructionType.BS, InstructionType.None, InstructionType.JMP,
                           "Verify ITERN at {D}; goto {D}"),

            // Returns.

            new("RETM", InstructionType.BS, InstructionType.None, InstructionType.LIT,
                         "return {from_A_x_D_minus_one}, ...MULTRES"),

            new("RET", InstructionType.RBS, InstructionType.None, InstructionType.LIT,
                        "return {from_A_x_D_minus_two}"),

            new("RET0", InstructionType.RBS, InstructionType.None, InstructionType.LIT, "return"),
            new("RET1", InstructionType.RBS, InstructionType.None, InstructionType.LIT, "return {A}"),

            // Loops and branches. I/J = interp/JIT, I/C/L = init/call/loop.

            new("FORI", InstructionType.BS, InstructionType.None, InstructionType.JMP,
                         "for {A_plus_three} = {A},{A_plus_one},{A_plus_two}\n else goto {D}"),

            new("JFORI", InstructionType.BS, InstructionType.None, InstructionType.JMP,
                          "for {A_plus_three} = {A},{A_plus_one},{A_plus_two}\n else goto {D}"),

            new("FORL", InstructionType.BS, InstructionType.None, InstructionType.JMP,
                         "{A} = {A} + {A_plus_two};\n if cmp({A}, sign {A_plus_two},  {A_plus_one}) goto {D}"),

            new("IFORL", InstructionType.BS, InstructionType.None, InstructionType.JMP,
                          "{A} = {A} + {A_plus_two};\n if cmp({A}, sign {A_plus_two}, {A_plus_one}) goto {D}"),

            new("JFORL", InstructionType.BS, InstructionType.None, InstructionType.JMP,
                          "{A} = {A} + {A_plus_two};\n if cmp({A}, sign {A_plus_two}, {A_plus_one}) goto {D}"),

            new("ITERL", InstructionType.BS, InstructionType.None, InstructionType.JMP,
                          "{A_minus_one} = {A}; if {A} != nil goto {D}"),

            new("IITERL", InstructionType.BS, InstructionType.None, InstructionType.JMP,
                           "{A_minus_one} = {A}; if {A} != nil goto {D}"),

            new("JITERL", InstructionType.BS, InstructionType.None, InstructionType.LIT,
                           "{A_minus_one} = {A}; if {A} != nil goto {D}"),

            new("LOOP", InstructionType.RBS, InstructionType.None, InstructionType.JMP, "Loop start, exit goto {D}"),
            new("ILOOP", InstructionType.RBS, InstructionType.None, InstructionType.JMP, "Noop"),
            new("JLOOP", InstructionType.RBS, InstructionType.None, InstructionType.LIT, "Noop"),

            new("JMP", InstructionType.RBS, InstructionType.None, InstructionType.JMP, "	goto {D}"),

            // Function headers. I/J = interp/JIT, F/V/C = fixarg/vararg/C func.
            // Shouldn't be ever seen - they are not stored in raw dump?

            new("FUNCF", InstructionType.RBS, InstructionType.None, InstructionType.None,
                          "Fixed-arg function with frame size {A}"),

            new("IFUNCF", InstructionType.RBS, InstructionType.None, InstructionType.None,
                           "Interpreted fixed-arg function with frame size {A}"),

            new("JFUNCF", InstructionType.RBS, InstructionType.None, InstructionType.LIT,
                           "JIT compiled fixed-arg function with frame size {A}"),

            new("FUNCV", InstructionType.RBS, InstructionType.None, InstructionType.None,
                          "Var-arg function with frame size {A}"),

            new("IFUNCV", InstructionType.RBS, InstructionType.None, InstructionType.None,
                           "Interpreted var-arg function with frame size {A}"),

            new("JFUNCV", InstructionType.RBS, InstructionType.None, InstructionType.LIT,
                           "JIT compiled var-arg function with frame size {A}"),

            new("FUNCC", InstructionType.RBS, InstructionType.None, InstructionType.None,
                          "C function with frame size {A}"),
            new("FUNCCW", InstructionType.RBS, InstructionType.None, InstructionType.None,
                           "Wrapped C function with frame size {A}"),

            ]);

        private static readonly string[] _opcodeMapV20 = ["ISLT",
            "ISGE",
            "ISLE",
            "ISGT",

            "ISEQV",
            "ISNEV",

            "ISEQS",
            "ISNES",

            "ISEQN",
            "ISNEN",

            "ISEQP",
            "ISNEP",

            // Unary test and copy ops

            "ISTC",
            "ISFC",

            "IST",
            "ISF",

            // Unary ops

            "MOV",
            "NOT",
            "UNM",
            "LEN",

            // Binary ops

            "ADDVN",
            "SUBVN",
            "MULVN",
            "DIVVN",
            "MODVN",

            "ADDNV",
            "SUBNV",
            "MULNV",
            "DIVNV",
            "MODNV",

            "ADDVV",
            "SUBVV",
            "MULVV",
            "DIVVV",
            "MODVV",

            "POW",
            "CAT",

            // Constant ops

            "KSTR",
            "KCDATA",
            "KSHORT",
            "KNUM",
            "KPRI",

            "KNIL",

            // Upvalue and function ops

            "UGET",

            "USETV",
            "USETS",
            "USETN",
            "USETP",

            "UCLO",

            "FNEW",
            // Table ops

            "TNEW",

            "TDUP",

            "GGET",
            "GSET",

            "TGETV",
            "TGETS",
            "TGETB",

            "TSETV",
            "TSETS",
            "TSETB",

            "TSETM",

            // Calls and vararg handling

            "CALLM",
            "CALL",
            "CALLMT",
            "CALLT",

            "ITERC",
            "ITERN",

            "VARG",

            "ISNEXT",

            // Returns

            "RETM",
            "RET",
            "RET0",
            "RET1",

            // Loops and branches

            "FORI",
            "JFORI",

            "FORL",
            "IFORL",
            "JFORL",

            "ITERL",
            "IITERL",
            "JITERL",

            "LOOP",
            "ILOOP",
            "JLOOP",

            "JMP",

            // Function headers

            "FUNCF",
            "IFUNCF",
            "JFUNCF",

            "FUNCV",
            "IFUNCV",
            "JFUNCV",

            "FUNCC",
            "FUNCCW",];

        private static readonly string[] _opcodeMapV21 = [
            "ISLT",
            "ISGE",
            "ISLE",
            "ISGT",

            "ISEQV",
            "ISNEV",

            "ISEQS",
            "ISNES",

            "ISEQN",
            "ISNEN",

            "ISEQP",
            "ISNEP",

            // Unary test and copy ops

            "ISTC",
            "ISFC",

            "IST",
            "ISF",
            "ISTYPE",
            "ISNUM",

            // Unary ops

            "MOV",
            "NOT",
            "UNM",
            "LEN",

            // Binary ops

            "ADDVN",
            "SUBVN",
            "MULVN",
            "DIVVN",
            "MODVN",

            "ADDNV",
            "SUBNV",
            "MULNV",
            "DIVNV",
            "MODNV",

            "ADDVV",
            "SUBVV",
            "MULVV",
            "DIVVV",
            "MODVV",

            "POW",
            "CAT",

            // Constant ops

            "KSTR",
            "KCDATA",
            "KSHORT",
            "KNUM",
            "KPRI",

            "KNIL",

            // Upvalue and function ops

            "UGET",

            "USETV",
            "USETS",
            "USETN",
            "USETP",

            "UCLO",

            "FNEW",

            // Table ops

            "TNEW",

            "TDUP",

            "GGET",
            "GSET",

            "TGETV",
            "TGETS",
            "TGETB",
            "TGETR",

            "TSETV",
            "TSETS",
            "TSETB",

            "TSETM",
            "TSETR",

            // Calls and vararg handling

            "CALLM",
            "CALL",
            "CALLMT",
            "CALLT",

            "ITERC",
            "ITERN",

            "VARG",

            "ISNEXT",

            // Returns

            "RETM",
            "RET",
            "RET0",
            "RET1",

            // Loops and branches

            "FORI",
            "JFORI",

            "FORL",
            "IFORL",
            "JFORL",

            "ITERL",
            "IITERL",
            "JITERL",

            "LOOP",
            "ILOOP",
            "JLOOP",

            "JMP",

            // Function headers

            "FUNCF",
            "IFUNCF",
            "JFUNCF",

            "FUNCV",
            "IFUNCV",
            "JFUNCV",

            "FUNCC",
            "FUNCCW"
        ];

        private InstructionDefined GetInstructionDefined(byte opcode, bool is21 = true)
        {
            var maps = is21 ? _opcodeMapV21 : _opcodeMapV20;
            var name = maps.Length > opcode ? maps[opcode] : string.Empty;
            if (!string.IsNullOrEmpty(name))
            {
                var items = _instructionItems.Value;
                foreach (var item in items)
                {
                    if (item.Name == name)
                    {
                        item.Opcode = opcode;
                        return item;
                    }
                }
            }
            return new("UNKNW", InstructionType.LIT, InstructionType.LIT, InstructionType.LIT, "Unknown instruction")
            {
                Opcode = opcode,
            };
        }
    }
}
