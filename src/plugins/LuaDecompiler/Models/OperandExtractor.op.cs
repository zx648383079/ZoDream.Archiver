namespace ZoDream.LuaDecompiler.Models
{
    public partial class OperandExtractor
    {
        private static readonly Operand[] _opcodeMapV50 = [
            Operand.MOVE,/*	A B	R(A) := R(B)					*/
            Operand.LOADK,/*	A Bx	R(A) := Kst(Bx)					*/
            Operand.LOADBOOL,/*	A B C	R(A) := (Bool)B; if (C) PC++			*/
            Operand.LOADNIL,/*	A B	R(A) := ... := R(B) := nil			*/
            Operand.GETUPVAL,/*	A B	R(A) := UpValue[B]				*/

            Operand.GETGLOBAL,/*	A Bx	R(A) := Gbl[Kst(Bx)]				*/
            Operand.GETTABLE,/*	A B C	R(A) := R(B)[RK(C)]				*/

            Operand.SETGLOBAL,/*	A Bx	Gbl[Kst(Bx)] := R(A)				*/
            Operand.SETUPVAL,/*	A B	UpValue[B] := R(A)				*/
            Operand.SETTABLE,/*	A B C	R(A)[RK(B)] := RK(C)				*/

            Operand.NEWTABLE,/*	A B C	R(A) := {} (size = B,C)				*/

            Operand.SELF,/*	A B C	R(A+1) := R(B); R(A) := R(B)[RK(C)]		*/

            Operand.ADD,/*	A B C	R(A) := RK(B) + RK(C)				*/
            Operand.SUB,/*	A B C	R(A) := RK(B) - RK(C)				*/
            Operand.MUL,/*	A B C	R(A) := RK(B) * RK(C)				*/
            Operand.DIV,/*	A B C	R(A) := RK(B) / RK(C)				*/
            Operand.POW,/*	A B C	R(A) := RK(B) ^ RK(C)				*/
            Operand.UNM,/*	A B	R(A) := -R(B)					*/
            Operand.NOT,/*	A B	R(A) := not R(B)				*/

            Operand.CONCAT,/*	A B C	R(A) := R(B).. ... ..R(C)			*/

            Operand.JMP,/*	sBx	PC += sBx					*/

            Operand.EQ,/*	A B C	if ((RK(B) == RK(C)) ~= A) then pc++		*/
            Operand.LT,/*	A B C	if ((RK(B) <  RK(C)) ~= A) then pc++  		*/
            Operand.LE,/*	A B C	if ((RK(B) <= RK(C)) ~= A) then pc++  		*/

            Operand.TEST,/*	A B C	if (R(B) <=> C) then R(A) := R(B) else pc++	*/ 

            Operand.CALL,/*	A B C	R(A), ... ,R(A+C-2) := R(A)(R(A+1), ... ,R(A+B-1)) */
            Operand.TAILCALL,/*	A B C	return R(A)(R(A+1), ... ,R(A+B-1))		*/
            Operand.RETURN,/*	A B	return R(A), ... ,R(A+B-2)	(see note)	*/

            Operand.FORLOOP,/*	A sBx	R(A)+=R(A+2); if R(A) <?= R(A+1) then PC+= sBx	*/

            Operand.TFORLOOP,/*	A C	R(A+2), ... ,R(A+2+C) := R(A)(R(A+1), R(A+2)); 
                                    if R(A+2) ~= nil then pc++			*/
            Operand.TFORPREP,/*	A sBx	if type(R(A)) == table then R(A+1):=R(A), R(A):=next;
			            PC += sBx					*/

            Operand.SETLIST,/*	A Bx	R(A)[Bx-Bx%FPF+i] := R(A+i), 1 <= i <= Bx%FPF+1	*/
            Operand.SETLISTO,/*	A Bx							*/

            Operand.CLOSE,/*	A 	close all variables in the stack up to (>=) R(A)*/
            Operand.CLOSURE/*	A Bx	R(A) := closure(KPROTO[Bx], R(A), ... ,R(A+n))	*/
        ];
        private static readonly Operand[] _opcodeMapV51 = [
            Operand.MOVE,/*	A B	R(A) := R(B)					*/
            Operand.LOADK,/*	A Bx	R(A) := Kst(Bx)					*/
            Operand.LOADBOOL,/*	A B C	R(A) := (Bool)B; if (C) pc++			*/
            Operand.LOADNIL,/*	A B	R(A) := ... := R(B) := nil			*/
            Operand.GETUPVAL,/*	A B	R(A) := UpValue[B]				*/
    
            Operand.GETGLOBAL,/*	A Bx	R(A) := Gbl[Kst(Bx)]				*/
            Operand.GETTABLE,/*	A B C	R(A) := R(B)[RK(C)]				*/
    
            Operand.SETGLOBAL,/*	A Bx	Gbl[Kst(Bx)] := R(A)				*/
            Operand.SETUPVAL,/*	A B	UpValue[B] := R(A)				*/
            Operand.SETTABLE,/*	A B C	R(A)[RK(B)] := RK(C)				*/
    
            Operand.NEWTABLE,/*	A B C	R(A) := {} (size = B,C)				*/
    
            Operand.SELF,/*	A B C	R(A+1) := R(B); R(A) := R(B)[RK(C)]		*/
    
            Operand.ADD,/*	A B C	R(A) := RK(B) + RK(C)				*/
            Operand.SUB,/*	A B C	R(A) := RK(B) - RK(C)				*/
            Operand.MUL,/*	A B C	R(A) := RK(B) * RK(C)				*/
            Operand.DIV,/*	A B C	R(A) := RK(B) / RK(C)				*/
            Operand.MOD,/*	A B C	R(A) := RK(B) % RK(C)				*/
            Operand.POW,/*	A B C	R(A) := RK(B) ^ RK(C)				*/
            Operand.UNM,/*	A B	R(A) := -R(B)					*/
            Operand.NOT,/*	A B	R(A) := not R(B)				*/
            Operand.LEN,/*	A B	R(A) := length of R(B)				*/
    
            Operand.CONCAT,/*	A B C	R(A) := R(B).. ... ..R(C)			*/
    
            Operand.JMP,/*	sBx	pc+=sBx					*/
    
            Operand.EQ,/*	A B C	if ((RK(B) == RK(C)) ~= A) then pc++		*/
            Operand.LT,/*	A B C	if ((RK(B) <  RK(C)) ~= A) then pc++  		*/
            Operand.LE,/*	A B C	if ((RK(B) <= RK(C)) ~= A) then pc++  		*/
    
            Operand.TEST,/*	A C	if not (R(A) <=> C) then pc++			*/ 
            Operand.TESTSET,/*	A B C	if (R(B) <=> C) then R(A) := R(B) else pc++	*/ 
    
            Operand.CALL,/*	A B C	R(A), ... ,R(A+C-2) := R(A)(R(A+1), ... ,R(A+B-1)) */
            Operand.TAILCALL,/*	A B C	return R(A)(R(A+1), ... ,R(A+B-1))		*/
            Operand.RETURN,/*	A B	return R(A), ... ,R(A+B-2)	(see note)	*/
    
            Operand.FORLOOP,/*	A sBx	R(A)+=R(A+2);
                        if R(A) <?= R(A+1) then { pc+=sBx; R(A+3)=R(A) }*/
            Operand.FORPREP,/*	A sBx	R(A)-=R(A+2); pc+=sBx				*/
    
            Operand.TFORLOOP,/*	A C	R(A+3), ... ,R(A+2+C) := R(A)(R(A+1), R(A+2)); 
                                    if R(A+3) ~= nil then R(A+2)=R(A+3) else pc++	*/ 
            Operand.SETLIST,/*	A B C	R(A)[(C-1)*FPF+i] := R(A+i), 1 <= i <= B	*/
    
            Operand.CLOSE,/*	A 	close all variables in the stack up to (>=) R(A)*/
            Operand.CLOSURE,/*	A Bx	R(A) := closure(KPROTO[Bx], R(A), ... ,R(A+n))	*/
    
            Operand.VARARG/*	A B	R(A), R(A+1), ..., R(A+B-1) = vararg		*/
        ];
        private static readonly Operand[] _opcodeMapV52 = [
            Operand.MOVE,/*	A B	R(A) := R(B)					*/
            Operand.LOADK,/*	A Bx	R(A) := Kst(Bx)					*/
            Operand.LOADKX,/*	A 	R(A) := Kst(extra arg)				*/
            Operand.LOADBOOL,/*	A B C	R(A) := (Bool)B; if (C) pc++			*/
            Operand.LOADNIL,/*	A B	R(A), R(A+1), ..., R(A+B) := nil		*/
            Operand.GETUPVAL,/*	A B	R(A) := UpValue[B]				*/

            Operand.GETTABUP,/*	A B C	R(A) := UpValue[B][RK(C)]			*/
            Operand.GETTABLE,/*	A B C	R(A) := R(B)[RK(C)]				*/

            Operand.SETTABUP,/*	A B C	UpValue[A][RK(B)] := RK(C)			*/
            Operand.SETUPVAL,/*	A B	UpValue[B] := R(A)				*/
            Operand.SETTABLE,/*	A B C	R(A)[RK(B)] := RK(C)				*/

            Operand.NEWTABLE,/*	A B C	R(A) := {} (size = B,C)				*/

            Operand.SELF,/*	A B C	R(A+1) := R(B); R(A) := R(B)[RK(C)]		*/

            Operand.ADD,/*	A B C	R(A) := RK(B) + RK(C)				*/
            Operand.SUB,/*	A B C	R(A) := RK(B) - RK(C)				*/
            Operand.MUL,/*	A B C	R(A) := RK(B) * RK(C)				*/
            Operand.DIV,/*	A B C	R(A) := RK(B) / RK(C)				*/
            Operand.MOD,/*	A B C	R(A) := RK(B) % RK(C)				*/
            Operand.POW,/*	A B C	R(A) := RK(B) ^ RK(C)				*/
            Operand.UNM,/*	A B	R(A) := -R(B)					*/
            Operand.NOT,/*	A B	R(A) := not R(B)				*/
            Operand.LEN,/*	A B	R(A) := length of R(B)				*/

            Operand.CONCAT,/*	A B C	R(A) := R(B).. ... ..R(C)			*/

            Operand.JMP,/*	A sBx	pc+=sBx; if (A) close all upvalues >= R(A - 1)	*/
            Operand.EQ,/*	A B C	if ((RK(B) == RK(C)) ~= A) then pc++		*/
            Operand.LT,/*	A B C	if ((RK(B) <  RK(C)) ~= A) then pc++		*/
            Operand.LE,/*	A B C	if ((RK(B) <= RK(C)) ~= A) then pc++		*/

            Operand.TEST,/*	A C	if not (R(A) <=> C) then pc++			*/
            Operand.TESTSET,/*	A B C	if (R(B) <=> C) then R(A) := R(B) else pc++	*/

            Operand.CALL,/*	A B C	R(A), ... ,R(A+C-2) := R(A)(R(A+1), ... ,R(A+B-1)) */
            Operand.TAILCALL,/*	A B C	return R(A)(R(A+1), ... ,R(A+B-1))		*/
            Operand.RETURN,/*	A B	return R(A), ... ,R(A+B-2)	(see note)	*/

            Operand.FORLOOP,/*	A sBx	R(A)+=R(A+2);
			            if R(A) <?= R(A+1) then { pc+=sBx; R(A+3)=R(A) }*/
            Operand.FORPREP,/*	A sBx	R(A)-=R(A+2); pc+=sBx				*/

            Operand.TFORCALL,/*	A C	R(A+3), ... ,R(A+2+C) := R(A)(R(A+1), R(A+2));	*/
            Operand.TFORLOOP,/*	A sBx	if R(A+1) ~= nil then { R(A)=R(A+1); pc += sBx }*/

            Operand.SETLIST,/*	A B C	R(A)[(C-1)*FPF+i] := R(A+i), 1 <= i <= B	*/

            Operand.CLOSURE,/*	A Bx	R(A) := closure(KPROTO[Bx])			*/

            Operand.VARARG,/*	A B	R(A), R(A+1), ..., R(A+B-2) = vararg		*/

            Operand.EXTRAARG/*	Ax	extra (larger) argument for previous opcode	*/
        ];
        private static readonly Operand[] _opcodeMapV53 = [
            Operand.MOVE,/*	A B	R(A) := R(B)					*/
            Operand.LOADK,/*	A Bx	R(A) := Kst(Bx)					*/
            Operand.LOADKX,/*	A 	R(A) := Kst(extra arg)				*/
            Operand.LOADBOOL,/*	A B C	R(A) := (Bool)B; if (C) pc++			*/
            Operand.LOADNIL,/*	A B	R(A), R(A+1), ..., R(A+B) := nil		*/
            Operand.GETUPVAL,/*	A B	R(A) := UpValue[B]				*/

            Operand.GETTABUP,/*	A B C	R(A) := UpValue[B][RK(C)]			*/
            Operand.GETTABLE,/*	A B C	R(A) := R(B)[RK(C)]				*/

            Operand.SETTABUP,/*	A B C	UpValue[A][RK(B)] := RK(C)			*/
            Operand.SETUPVAL,/*	A B	UpValue[B] := R(A)				*/
            Operand.SETTABLE,/*	A B C	R(A)[RK(B)] := RK(C)				*/

            Operand.NEWTABLE,/*	A B C	R(A) := {} (size = B,C)				*/

            Operand.SELF,/*	A B C	R(A+1) := R(B); R(A) := R(B)[RK(C)]		*/

            Operand.ADD,/*	A B C	R(A) := RK(B) + RK(C)				*/
            Operand.SUB,/*	A B C	R(A) := RK(B) - RK(C)				*/
            Operand.MUL,/*	A B C	R(A) := RK(B) * RK(C)				*/
            Operand.MOD,/*	A B C	R(A) := RK(B) % RK(C)				*/
            Operand.POW,/*	A B C	R(A) := RK(B) ^ RK(C)				*/
            Operand.DIV,/*	A B C	R(A) := RK(B) / RK(C)				*/
            Operand.IDIV,/*	A B C	R(A) := RK(B) // RK(C)				*/
            Operand.BAND,/*	A B C	R(A) := RK(B) & RK(C)				*/
            Operand.BOR,/*	A B C	R(A) := RK(B) | RK(C)				*/
            Operand.BXOR,/*	A B C	R(A) := RK(B) ~ RK(C)				*/
            Operand.SHL,/*	A B C	R(A) := RK(B) << RK(C)				*/
            Operand.SHR,/*	A B C	R(A) := RK(B) >> RK(C)				*/
            Operand.UNM,/*	A B	R(A) := -R(B)					*/
            Operand.BNOT,/*	A B	R(A) := ~R(B)					*/
            Operand.NOT,/*	A B	R(A) := not R(B)				*/
            Operand.LEN,/*	A B	R(A) := length of R(B)				*/

            Operand.CONCAT,/*	A B C	R(A) := R(B).. ... ..R(C)			*/

            Operand.JMP,/*	A sBx	pc+=sBx; if (A) close all upvalues >= R(A - 1)	*/
            Operand.EQ,/*	A B C	if ((RK(B) == RK(C)) ~= A) then pc++		*/
            Operand.LT,/*	A B C	if ((RK(B) <  RK(C)) ~= A) then pc++		*/
            Operand.LE,/*	A B C	if ((RK(B) <= RK(C)) ~= A) then pc++		*/

            Operand.TEST,/*	A C	if not (R(A) <=> C) then pc++			*/
            Operand.TESTSET,/*	A B C	if (R(B) <=> C) then R(A) := R(B) else pc++	*/

            Operand.CALL,/*	A B C	R(A), ... ,R(A+C-2) := R(A)(R(A+1), ... ,R(A+B-1)) */
            Operand.TAILCALL,/*	A B C	return R(A)(R(A+1), ... ,R(A+B-1))		*/
            Operand.RETURN,/*	A B	return R(A), ... ,R(A+B-2)	(see note)	*/

            Operand.FORLOOP,/*	A sBx	R(A)+=R(A+2);
			            if R(A) <?= R(A+1) then { pc+=sBx; R(A+3)=R(A) }*/
            Operand.FORPREP,/*	A sBx	R(A)-=R(A+2); pc+=sBx				*/

            Operand.TFORCALL,/*	A C	R(A+3), ... ,R(A+2+C) := R(A)(R(A+1), R(A+2));	*/
            Operand.TFORLOOP,/*	A sBx	if R(A+1) ~= nil then { R(A)=R(A+1); pc += sBx }*/

            Operand.SETLIST,/*	A B C	R(A)[(C-1)*FPF+i] := R(A+i), 1 <= i <= B	*/

            Operand.CLOSURE,/*	A Bx	R(A) := closure(KPROTO[Bx])			*/

            Operand.VARARG,/*	A B	R(A), R(A+1), ..., R(A+B-2) = vararg		*/

            Operand.EXTRAARG/*	Ax	extra (larger) argument for previous opcode	*/
        ];
        private static readonly Operand[] _opcodeMapV54 = [
            Operand.MOVE,/*	A B	R[A] := R[B]					*/
            Operand.LOADI,/*	A sBx	R[A] := sBx					*/
            Operand.LOADF,/*	A sBx	R[A] := (lua_Number)sBx				*/
            Operand.LOADK,/*	A Bx	R[A] := K[Bx]					*/
            Operand.LOADKX,/*	A	R[A] := K[extra arg]				*/
            Operand.LOADFALSE,/*	A	R[A] := false					*/
            Operand.LFALSESKIP,/*A	R[A] := false; pc++	(*)			*/
            Operand.LOADTRUE,/*	A	R[A] := true					*/
            Operand.LOADNIL,/*	A B	R[A], R[A+1], ..., R[A+B] := nil		*/
            Operand.GETUPVAL,/*	A B	R[A] := UpValue[B]				*/
            Operand.SETUPVAL,/*	A B	UpValue[B] := R[A]				*/

            Operand.GETTABUP,/*	A B C	R[A] := UpValue[B][K[C]:shortstring]		*/
            Operand.GETTABLE,/*	A B C	R[A] := R[B][R[C]]				*/
            Operand.GETI,/*	A B C	R[A] := R[B][C]					*/
            Operand.GETFIELD,/*	A B C	R[A] := R[B][K[C]:shortstring]			*/

            Operand.SETTABUP,/*	A B C	UpValue[A][K[B]:shortstring] := RK(C)		*/
            Operand.SETTABLE,/*	A B C	R[A][R[B]] := RK(C)				*/
            Operand.SETI,/*	A B C	R[A][B] := RK(C)				*/
            Operand.SETFIELD,/*	A B C	R[A][K[B]:shortstring] := RK(C)			*/

            Operand.NEWTABLE,/*	A B C k	R[A] := {}					*/

            Operand.SELF,/*	A B C	R[A+1] := R[B]; R[A] := R[B][RK(C):string]	*/

            Operand.ADDI,/*	A B sC	R[A] := R[B] + sC				*/

            Operand.ADDK,/*	A B C	R[A] := R[B] + K[C]:number			*/
            Operand.SUBK,/*	A B C	R[A] := R[B] - K[C]:number			*/
            Operand.MULK,/*	A B C	R[A] := R[B] * K[C]:number			*/
            Operand.MODK,/*	A B C	R[A] := R[B] % K[C]:number			*/
            Operand.POWK,/*	A B C	R[A] := R[B] ^ K[C]:number			*/
            Operand.DIVK,/*	A B C	R[A] := R[B] / K[C]:number			*/
            Operand.IDIVK,/*	A B C	R[A] := R[B] // K[C]:number			*/

            Operand.BANDK,/*	A B C	R[A] := R[B] & K[C]:integer			*/
            Operand.BORK,/*	A B C	R[A] := R[B] | K[C]:integer			*/
            Operand.BXORK,/*	A B C	R[A] := R[B] ~ K[C]:integer			*/

            Operand.SHRI,/*	A B sC	R[A] := R[B] >> sC				*/
            Operand.SHLI,/*	A B sC	R[A] := sC << R[B]				*/

            Operand.ADD,/*	A B C	R[A] := R[B] + R[C]				*/
            Operand.SUB,/*	A B C	R[A] := R[B] - R[C]				*/
            Operand.MUL,/*	A B C	R[A] := R[B] * R[C]				*/
            Operand.MOD,/*	A B C	R[A] := R[B] % R[C]				*/
            Operand.POW,/*	A B C	R[A] := R[B] ^ R[C]				*/
            Operand.DIV,/*	A B C	R[A] := R[B] / R[C]				*/
            Operand.IDIV,/*	A B C	R[A] := R[B] // R[C]				*/

            Operand.BAND,/*	A B C	R[A] := R[B] & R[C]				*/
            Operand.BOR,/*	A B C	R[A] := R[B] | R[C]				*/
            Operand.BXOR,/*	A B C	R[A] := R[B] ~ R[C]				*/
            Operand.SHL,/*	A B C	R[A] := R[B] << R[C]				*/
            Operand.SHR,/*	A B C	R[A] := R[B] >> R[C]				*/

            Operand.MMBIN,/*	A B C	call C metamethod over R[A] and R[B]	(*)	*/
            Operand.MMBINI,/*	A sB C k	call C metamethod over R[A] and sB	*/
            Operand.MMBINK,/*	A B C k		call C metamethod over R[A] and K[B]	*/

            Operand.UNM,/*	A B	R[A] := -R[B]					*/
            Operand.BNOT,/*	A B	R[A] := ~R[B]					*/
            Operand.NOT,/*	A B	R[A] := not R[B]				*/
            Operand.LEN,/*	A B	R[A] := #R[B] (length operator)			*/

            Operand.CONCAT,/*	A B	R[A] := R[A].. ... ..R[A + B - 1]		*/

            Operand.CLOSE,/*	A	close all upvalues >= R[A]			*/
            Operand.TBC,/*	A	mark variable A "to be closed"			*/
            Operand.JMP,/*	sJ	pc += sJ					*/
            Operand.EQ,/*	A B k	if ((R[A] == R[B]) ~= k) then pc++		*/
            Operand.LT,/*	A B k	if ((R[A] <  R[B]) ~= k) then pc++		*/
            Operand.LE,/*	A B k	if ((R[A] <= R[B]) ~= k) then pc++		*/

            Operand.EQK,/*	A B k	if ((R[A] == K[B]) ~= k) then pc++		*/
            Operand.EQI,/*	A sB k	if ((R[A] == sB) ~= k) then pc++		*/
            Operand.LTI,/*	A sB k	if ((R[A] < sB) ~= k) then pc++			*/
            Operand.LEI,/*	A sB k	if ((R[A] <= sB) ~= k) then pc++		*/
            Operand.GTI,/*	A sB k	if ((R[A] > sB) ~= k) then pc++			*/
            Operand.GEI,/*	A sB k	if ((R[A] >= sB) ~= k) then pc++		*/

            Operand.TEST,/*	A k	if (not R[A] == k) then pc++			*/
            Operand.TESTSET,/*	A B k	if (not R[B] == k) then pc++ else R[A] := R[B] (*) */

            Operand.CALL,/*	A B C	R[A], ... ,R[A+C-2] := R[A](R[A+1], ... ,R[A+B-1]) */
            Operand.TAILCALL,/*	A B C k	return R[A](R[A+1], ... ,R[A+B-1])		*/

            Operand.RETURN,/*	A B C k	return R[A], ... ,R[A+B-2]	(see note)	*/
            Operand.RETURN0,/*		return						*/
            Operand.RETURN1,/*	A	return R[A]					*/

            Operand.FORLOOP,/*	A Bx	update counters; if loop continues then pc-=Bx; */
            Operand.FORPREP,/*	A Bx	<check values and prepare counters>;
                                    if not to run then pc+=Bx+1;			*/

            Operand.TFORPREP,/*	A Bx	create upvalue for R[A + 3]; pc+=Bx		*/
            Operand.TFORCALL,/*	A C	R[A+4], ... ,R[A+3+C] := R[A](R[A+1], R[A+2]);	*/
            Operand.TFORLOOP,/*	A Bx	if R[A+2] ~= nil then { R[A]=R[A+2]; pc -= Bx }	*/

            Operand.SETLIST,/*	A B C k	R[A][C+i] := R[A+i], 1 <= i <= B		*/

            Operand.CLOSURE,/*	A Bx	R[A] := closure(KPROTO[Bx])			*/

            Operand.VARARG,/*	A C	R[A], R[A+1], ..., R[A+C-2] = vararg		*/

            Operand.VARARGPREP,/*A	(adjust vararg parameters)			*/

            Operand.EXTRAARG/*	Ax	extra (larger) argument for previous opcode	*/
        ];
    }

}
