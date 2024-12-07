using ZoDream.LuaDecompiler.Attributes;

namespace ZoDream.LuaDecompiler.Models
{
    public enum JitOperand
    {
        [JitOperand("ISLT", JitOperandFormat.VAR, JitOperandFormat.None, JitOperandFormat.VAR)]
        ISLT, // if {A} < {D}
        [JitOperand("ISGE", JitOperandFormat.VAR, JitOperandFormat.None, JitOperandFormat.VAR)]
        ISGE, // if {A} >= {D}
        [JitOperand("ISLE", JitOperandFormat.VAR, JitOperandFormat.None, JitOperandFormat.VAR)]
        ISLE, // if {A} <= {D}
        [JitOperand("ISGT", JitOperandFormat.VAR, JitOperandFormat.None, JitOperandFormat.VAR)]
        ISGT, // if {A} > {D}

        [JitOperand("ISEQV", JitOperandFormat.VAR, JitOperandFormat.None, JitOperandFormat.VAR)]
        ISEQV, // if {A} == {D}
        [JitOperand("ISNEV", JitOperandFormat.VAR, JitOperandFormat.None, JitOperandFormat.VAR)]
        ISNEV, // if {A} ~= {D}

        [JitOperand("ISEQS", JitOperandFormat.VAR, JitOperandFormat.None, JitOperandFormat.STR)]
        ISEQS, // if {A} == {D}
        [JitOperand("ISNES", JitOperandFormat.VAR, JitOperandFormat.None, JitOperandFormat.STR)]
        ISNES, // if {A} ~= {D}

        [JitOperand("ISEQN", JitOperandFormat.VAR, JitOperandFormat.None, JitOperandFormat.NUM)]
        ISEQN, // if {A} == {D}
        [JitOperand("ISNEN", JitOperandFormat.VAR, JitOperandFormat.None, JitOperandFormat.NUM)]
        ISNEN, // if {A} ~= {D}

        [JitOperand("ISEQP", JitOperandFormat.VAR, JitOperandFormat.None, JitOperandFormat.PRI)]
        ISEQP, // if {A} == {D}
        [JitOperand("ISNEP", JitOperandFormat.VAR, JitOperandFormat.None, JitOperandFormat.PRI)]
        ISNEP, // if {A} ~= {D}

        // Unary test and copy ops

        [JitOperand("ISTC", JitOperandFormat.DST, JitOperandFormat.None, JitOperandFormat.VAR)]
        ISTC, // {A} = {D}; if {D}
        [JitOperand("ISFC", JitOperandFormat.DST, JitOperandFormat.None, JitOperandFormat.VAR)]
        ISFC, // {A} = {D}; if not {D}

        [JitOperand("IST", JitOperandFormat.None, JitOperandFormat.None, JitOperandFormat.VAR)]
        IST, // if {D}
        [JitOperand("ISF", JitOperandFormat.None, JitOperandFormat.None, JitOperandFormat.VAR)]
        ISF, // if not {D}

        // Added in bytecode version 2
        [JitOperand("ISTYPE", JitOperandFormat.VAR, JitOperandFormat.None, JitOperandFormat.LIT)]
        ISTYPE, // see lj vm source
        [JitOperand("ISNUM", JitOperandFormat.VAR, JitOperandFormat.None, JitOperandFormat.LIT)]
        ISNUM, // see lj vm source

        // Unary ops

        [JitOperand("MOV", JitOperandFormat.DST, JitOperandFormat.None, JitOperandFormat.VAR)]
        MOV, // {A} = {D}
        [JitOperand("NOT", JitOperandFormat.DST, JitOperandFormat.None, JitOperandFormat.VAR)]
        NOT, // {A} = not {D}
        [JitOperand("UNM", JitOperandFormat.DST, JitOperandFormat.None, JitOperandFormat.VAR)]
        UNM, // {A} = -{D}
        [JitOperand("LEN", JitOperandFormat.DST, JitOperandFormat.None, JitOperandFormat.VAR)]
        LEN, // {A} = #{D}

        // Binary ops

        [JitOperand("ADDVN", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.NUM)]
        ADDVN, // {A} = {B} + {C}
        [JitOperand("SUBVN", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.NUM)]
        SUBVN, // {A} = {B} - {C}
        [JitOperand("MULVN", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.NUM)]
        MULVN, // {A} = {B} * {C}
        [JitOperand("DIVVN", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.NUM)]
        DIVVN, // {A} = {B} / {C}
        [JitOperand("MODVN", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.NUM)]
        MODVN, // {A} = {B} % {C}

        [JitOperand("ADDNV", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.NUM)]
        ADDNV, // {A} = {C} + {B}
        [JitOperand("SUBNV", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.NUM)]
        SUBNV, // {A} = {C} - {B}
        [JitOperand("MULNV", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.NUM)]
        MULNV, // {A} = {C} * {B}
        [JitOperand("DIVNV", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.NUM)]
        DIVNV, // {A} = {C} / {B}
        [JitOperand("MODNV", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.NUM)]
        MODNV, // {A} = {C} % {B}

        [JitOperand("ADDVV", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.VAR)]
        ADDVV, // {A} = {B} + {C}
        [JitOperand("SUBVV", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.VAR)]
        SUBVV, // {A} = {B} - {C}
        [JitOperand("MULVV", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.VAR)]
        MULVV, // {A} = {B} * {C}
        [JitOperand("DIVVV", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.VAR)]
        DIVVV, // {A} = {B} / {C}
        [JitOperand("MODVV", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.VAR)]
        MODVV, // {A} = {B} % {C}

        [JitOperand("POW", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.VAR)]
        POW, // {A} = {B} ^ {C} (pow)
        [JitOperand("CAT", JitOperandFormat.DST, JitOperandFormat.RBS, JitOperandFormat.RBS)]
        CAT, // {A} = {concaJitOperandFormat.from_B_to_C}

        // Constant ops.

        [JitOperand("KSTR", JitOperandFormat.DST, JitOperandFormat.None, JitOperandFormat.STR)]
        KSTR, // {A} = {D}
        [JitOperand("KCDATA", JitOperandFormat.DST, JitOperandFormat.None, JitOperandFormat.CDT)]
        KCDATA, // {A} = {D}
        [JitOperand("KSHORT", JitOperandFormat.DST, JitOperandFormat.None, JitOperandFormat.SLIT)]
        KSHORT, // {A} = {D}
        [JitOperand("KNUM", JitOperandFormat.DST, JitOperandFormat.None, JitOperandFormat.NUM)]
        KNUM, // {A} = {D}
        [JitOperand("KPRI", JitOperandFormat.DST, JitOperandFormat.None, JitOperandFormat.PRI)]
        KPRI, // {A} = {D}

        [JitOperand("KNIL", JitOperandFormat.BS, JitOperandFormat.None, JitOperandFormat.BS)]
        KNIL, // {from_A_to_D} = nil

        // Upvalue and function ops.

        [JitOperand("UGET", JitOperandFormat.DST, JitOperandFormat.None, JitOperandFormat.UV)]
        UGET, // {A} = {D}

        [JitOperand("USETV", JitOperandFormat.UV, JitOperandFormat.None, JitOperandFormat.VAR)]
        USETV, // {A} = {D}
        [JitOperand("USETS", JitOperandFormat.UV, JitOperandFormat.None, JitOperandFormat.STR)]
        USETS, // {A} = {D}
        [JitOperand("USETN", JitOperandFormat.UV, JitOperandFormat.None, JitOperandFormat.NUM)]
        USETN, // {A} = {D}
        [JitOperand("USETP", JitOperandFormat.UV, JitOperandFormat.None, JitOperandFormat.PRI)]
        USETP, // {A} = {D}

        [JitOperand("UCLO", JitOperandFormat.RBS, JitOperandFormat.None, JitOperandFormat.JMP)]
        UCLO, // "nil uvs >= {A}; goto {D}"

        [JitOperand("FNEW", JitOperandFormat.DST, JitOperandFormat.None, JitOperandFormat.FUN)]
        FNEW, // {A} = function {D}

            // Table ops.

        [JitOperand("TNEW", JitOperandFormat.DST, JitOperandFormat.None, JitOperandFormat.LIT)]
        TNEW, // {A} = new table(\n array: {D_array},\n dict: {D_dict})

        [JitOperand("TDUP", JitOperandFormat.DST, JitOperandFormat.None, JitOperandFormat.TAB)]
        TDUP, // {A} = copy {D}

        [JitOperand("GGET", JitOperandFormat.DST, JitOperandFormat.None, JitOperandFormat.STR)]
        GGET, // {A} = _env[{D}]
        [JitOperand("GSET", JitOperandFormat.VAR, JitOperandFormat.None, JitOperandFormat.STR)]
        GSET, // _env[{D}] = {A}

        [JitOperand("TGETV", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.VAR)]
        TGETV, // {A} = {B}[{C}]
        [JitOperand("TGETS", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.STR)]
        TGETS, // {A} = {B}.{C}
        [JitOperand("TGETB", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.LIT)]
        TGETB, // {A} = {B}[{C}]

                    // Added in bytecode version 2
        [JitOperand("TGETR", JitOperandFormat.DST, JitOperandFormat.VAR, JitOperandFormat.VAR)]
        TGETR, // {A} = {B}[{C}]

        [JitOperand("TSETV", JitOperandFormat.VAR, JitOperandFormat.VAR, JitOperandFormat.VAR)]
        TSETV, // {B}[{C}] = {A}
        [JitOperand("TSETS", JitOperandFormat.VAR, JitOperandFormat.VAR, JitOperandFormat.STR)]
        TSETS, // {B}.{C} = {A}
        [JitOperand("TSETB", JitOperandFormat.VAR, JitOperandFormat.VAR, JitOperandFormat.LIT)]
        TSETB, // {B}[{C}] = {A}

        [JitOperand("TSETM", JitOperandFormat.BS, JitOperandFormat.None, JitOperandFormat.NUM)]
        TSETM, // for i = 0, MULTRES, 1 do\n {A_minus_one}[{D_low} + i] = slot({A} + i)

                    // Added in bytecode version 2
        [JitOperand("TSETR", JitOperandFormat.VAR, JitOperandFormat.VAR, JitOperandFormat.VAR)]
        TSETR, // {B}[{C}] = {A}

                    // Calls and vararg handling. T = tail call.

        [JitOperand("CALLM", JitOperandFormat.BS, JitOperandFormat.LIT, JitOperandFormat.LIT)]
        CALLM, // {from_A_x_B_minus_two} = {A}({from_A_plus_one_x_C}, ...MULTRES)

        [JitOperand("CALL", JitOperandFormat.BS, JitOperandFormat.LIT, JitOperandFormat.LIT)]
        CALL, // {from_A_x_B_minus_two} = {A}({from_A_plus_one_x_C_minus_one})

        [JitOperand("CALLMT", JitOperandFormat.BS, JitOperandFormat.None, JitOperandFormat.LIT)]
        CALLMT, // return {A}({from_A_plus_one_x_D}, ...MULTRES)

        [JitOperand("CALLT", JitOperandFormat.BS, JitOperandFormat.None, JitOperandFormat.LIT)]
        CALLT, // return {A}({from_A_plus_one_x_D_minus_one})

        [JitOperand("ITERC", JitOperandFormat.BS, JitOperandFormat.LIT, JitOperandFormat.LIT)]
        ITERC, // {A}, {A_plus_one}, {A_plus_two} =\n {A_minus_three}, {A_minus_two}, {A_minus_one};\n {from_A_x_B_minus_two} =\n {A_minus_three}({A_minus_two}, {A_minus_one})

        [JitOperand("ITERN", JitOperandFormat.BS, JitOperandFormat.LIT, JitOperandFormat.LIT)]
        ITERN, // {A}, {A_plus_one}, {A_plus_two} =\n {A_minus_three}, {A_minus_two}, {A_minus_one};\n {from_A_x_B_minus_two} =\n {A_minus_three}({A_minus_two}, {A_minus_one})

        [JitOperand("VARG", JitOperandFormat.BS, JitOperandFormat.LIT, JitOperandFormat.LIT)]
        VARG, // {from_A_x_B_minus_two} = ...

        [JitOperand("ISNEXT", JitOperandFormat.BS, JitOperandFormat.None, JitOperandFormat.JMP)]
        ISNEXT, // Verify ITERN at {D}; goto {D}

                    // Returns.

        [JitOperand("RETM", JitOperandFormat.BS, JitOperandFormat.None, JitOperandFormat.LIT)]
        RETM, // return {from_A_x_D_minus_one}, ...MULTRES

        [JitOperand("RET", JitOperandFormat.RBS, JitOperandFormat.None, JitOperandFormat.LIT)]
        RET, // return {from_A_x_D_minus_two}

        [JitOperand("RET0", JitOperandFormat.RBS, JitOperandFormat.None, JitOperandFormat.LIT)]
        RET0, // return
        [JitOperand("RET1", JitOperandFormat.RBS, JitOperandFormat.None, JitOperandFormat.LIT)]
        RET1, // return {A}

                    // Loops and branches. I/J = interp/JIT, I/C/L = init/call/loop.

        [JitOperand("FORI", JitOperandFormat.BS, JitOperandFormat.None, JitOperandFormat.JMP)]
        FORI, // for {A_plus_three} = {A},{A_plus_one},{A_plus_two}\n else goto {D}

        [JitOperand("JFORI", JitOperandFormat.BS, JitOperandFormat.None, JitOperandFormat.JMP)]
        JFORI, // for {A_plus_three} = {A},{A_plus_one},{A_plus_two}\n else goto {D}

        [JitOperand("FORL", JitOperandFormat.BS, JitOperandFormat.None, JitOperandFormat.JMP)]
        FORL, // {A} = {A} + {A_plus_two};\n if cmp({A}, sign {A_plus_two},  {A_plus_one}) goto {D}

        [JitOperand("IFORL", JitOperandFormat.BS, JitOperandFormat.None, JitOperandFormat.JMP)]
        IFORL, // {A} = {A} + {A_plus_two};\n if cmp({A}, sign {A_plus_two}, {A_plus_one}) goto {D}

        [JitOperand("JFORL", JitOperandFormat.BS, JitOperandFormat.None, JitOperandFormat.JMP)]
        JFORL, // {A} = {A} + {A_plus_two};\n if cmp({A}, sign {A_plus_two}, {A_plus_one}) goto {D}

        [JitOperand("ITERL", JitOperandFormat.BS, JitOperandFormat.None, JitOperandFormat.JMP)]
        ITERL, // {A_minus_one} = {A}; if {A} != nil goto {D}

        [JitOperand("IITERL", JitOperandFormat.BS, JitOperandFormat.None, JitOperandFormat.JMP)]
        IITERL, // {A_minus_one} = {A}; if {A} != nil goto {D}

        [JitOperand("JITERL", JitOperandFormat.BS, JitOperandFormat.None, JitOperandFormat.LIT)]
        JITERL, // {A_minus_one} = {A}; if {A} != nil goto {D}

        [JitOperand("LOOP", JitOperandFormat.RBS, JitOperandFormat.None, JitOperandFormat.JMP)]
        LOOP, // Loop start, exit goto {D}
        [JitOperand("ILOOP", JitOperandFormat.RBS, JitOperandFormat.None, JitOperandFormat.JMP)]
        ILOOP, // Noop
        [JitOperand("JLOOP", JitOperandFormat.RBS, JitOperandFormat.None, JitOperandFormat.LIT)]
        JLOOP, // Noop

        [JitOperand("JMP", JitOperandFormat.RBS, JitOperandFormat.None, JitOperandFormat.JMP)]
        JMP, // 	goto {D}

                    // Function headers. I/J = interp/JIT, F/V/C = fixarg/vararg/C func.
                    // Shouldn't be ever seen - they are not stored in raw dump?

        [JitOperand("FUNCF", JitOperandFormat.RBS, JitOperandFormat.None, JitOperandFormat.None)]
        FUNCF, // Fixed-arg function with frame size {A}

        [JitOperand("IFUNCF", JitOperandFormat.RBS, JitOperandFormat.None, JitOperandFormat.None)]
        IFUNCF, // Interpreted fixed-arg function with frame size {A}

        [JitOperand("JFUNCF", JitOperandFormat.RBS, JitOperandFormat.None, JitOperandFormat.LIT)]
        JFUNCF, // JIT compiled fixed-arg function with frame size {A}

        [JitOperand("FUNCV", JitOperandFormat.RBS, JitOperandFormat.None, JitOperandFormat.None)]
        FUNCV, // Var-arg function with frame size {A}

        [JitOperand("IFUNCV", JitOperandFormat.RBS, JitOperandFormat.None, JitOperandFormat.None)]
        IFUNCV, // Interpreted var-arg function with frame size {A}

        [JitOperand("JFUNCV", JitOperandFormat.RBS, JitOperandFormat.None, JitOperandFormat.LIT)]
        JFUNCV, // JIT compiled var-arg function with frame size {A}

        [JitOperand("FUNCC", JitOperandFormat.RBS, JitOperandFormat.None, JitOperandFormat.None)]
        FUNCC, // C function with frame size {A}
        [JitOperand("FUNCCW", JitOperandFormat.RBS, JitOperandFormat.None, JitOperandFormat.None)]
        FUNCCW, // Wrapped C function with frame size {A}
        [JitOperand("UNKNW", JitOperandFormat.LIT, JitOperandFormat.LIT, JitOperandFormat.LIT)]
        UNKNW // Unknown instruction
    }
}
