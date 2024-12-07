use std::{cell::RefCell, collections::VecDeque};

use luau::read_array_count;

use super::*;

/* Header flags of bytecode */
pub const FLAG_IS_BIG_ENDIAN: u8 = 0b00000001;
pub const FLAG_IS_STRIPPED: u8 = 0b00000010;
pub const FLAG_HAS_FFI: u8 = 0b00000100;
// for luajit2.x
pub const FLAG_F_FR2: u8 = 0x08;

/* Flags for prototype. */
pub const PROTO_CHILD: u8 = 0x01; /* Has child prototypes. */
pub const PROTO_VARARG: u8 = 0x02; /* Vararg function. */
pub const PROTO_FFI: u8 = 0x04; /* Uses BC_KCDATA for FFI datatypes. */
pub const PROTO_NOJIT: u8 = 0x08; /* JIT disabled for this function. */
pub const PROTO_ILOOP: u8 = 0x10; /* Patched bytecode with ILOOP etc. */
// /* Only used during parsing. */
pub const PROTO_HAS_RETURN: u8 = 0x20; /* Already emitted a return. */
pub const PROTO_FIXUP_RETURN: u8 = 0x40; /* Need to fixup emitted returns. */

pub const BCDUMP_KGC_CHILD: u64 = 0;
pub const BCDUMP_KGC_TAB: u64 = 1;
pub const BCDUMP_KGC_I64: u64 = 2;
pub const BCDUMP_KGC_U64: u64 = 3;
pub const BCDUMP_KGC_COMPLEX: u64 = 4;
pub const BCDUMP_KGC_STR: u64 = 5;

pub const BCDUMP_KTAB_NIL: usize = 0;
pub const BCDUMP_KTAB_FALSE: usize = 1;
pub const BCDUMP_KTAB_TRUE: usize = 2;
pub const BCDUMP_KTAB_INT: usize = 3;
pub const BCDUMP_KTAB_NUM: usize = 4;
pub const BCDUMP_KTAB_STR: usize = 5;

const VARNAME_END: u8 = 0;
const VARNAME_FOR_IDX: u8 = 1;
const VARNAME_FOR_STOP: u8 = 2;
const VARNAME_FOR_STEP: u8 = 3;
const VARNAME_FOR_GEN: u8 = 4;
const VARNAME_FOR_STATE: u8 = 5;
const VARNAME_FOR_CTL: u8 = 6;
const VARNAME_MAX: u8 = 7;

const INTERNAL_VARNAMES: [&str; 7] = ["", "<index>", "<limit>", "<step>", "<generator>", "<state>", "<control>", ];

pub fn has_luajit_flag(header: & LuaHeader, flag: u8) -> bool {
    header.lj_flags & flag != 0
}


pub fn lj_header(input: & mut Cursor<&[u8]>) -> Result<LuaHeader> {
    let magic = input.read_bytes(3)?;
    if magic != b"\x1bLJ" {
        return Err(Error::form_str("lj magic error"));
    }
    let version = input.read_u8()?;
    
    match version  {
        0x1 | 0x2 => {
            let lj_flags = input.read_u8()?;
            Ok(
                LuaHeader {
                    lua_version: if version == 0x2 {LUAJ2.0} else {LUAJ1.0},
                    format_version: 0,
                    big_endian: lj_flags & FLAG_IS_BIG_ENDIAN != 0,
                    int_size: 4,
                    size_t_size: 4,
                    instruction_size: 4,
                    number_size: 4,
                    number_integral: false,
                    lj_flags,
                }
            )
        },
        _ => Err(Error::form_str("version error"))
    }
}

pub fn lj_complex_constant<'a, 'h>(
    stack: &'h mut VecDeque<LuaChunk>,
    protos: &'h mut Vec<LuaChunk>,
    input: & mut Cursor<&[u8]>, 
    big_endian: bool
) -> Result<LuaConstant> {
    let ty = input.read_leb128_u64()?;
        
    match ty {
        BCDUMP_KGC_I64 => {
            let lo = input.read_leb128_u32()?;
            let hi = input.read_leb128_u32()?;
            Ok(LuaConstant::Number(LuaNumber::Integer(lo as i64 | ((hi as i64) << 32))))
        },
        BCDUMP_KGC_U64 =>{
            let lo = input.read_leb128_u32()?;
            let hi = input.read_leb128_u32()?;
            Ok(LuaConstant::Number(LuaNumber::Integer(
                (lo as u64 | ((hi as u64) << 32)) as i64,
            )))
        },
        BCDUMP_KGC_COMPLEX => {
            let lo = input.read_leb128_u32()?;
            let hi = input.read_leb128_u32()?;
            let lo = input.read_leb128_u32()?;
            let hi = input.read_leb128_u32()?;
            Ok(LuaConstant::Number(LuaNumber::Integer(
                (lo as u64 | ((hi as u64) << 32)) as i64,
            )))
        }
        BCDUMP_KGC_TAB => lj_tab(input, big_endian),
        BCDUMP_KGC_CHILD => match stack.pop_front() {
            Some(proto) => {
                let result = LuaConstant::Proto(protos.len());
                protos.push(proto);
                Ok(result)
            }
            None => Err(Error::form_str("pop proto")),
        },
        _ if ty >= BCDUMP_KGC_STR => {
            let len = ty - BCDUMP_KGC_STR;
            Ok(LuaConstant::String(input.read_bytes(len)?))
        }
        _ => unreachable!("BCDUMP_KGC: {ty}"),
    }
}

pub fn lj_tab<'a>(input: & mut Cursor<&[u8]>, big_endian: bool) -> Result<LuaConstant> {
    let narray = input.read_leb128_u32()? as usize;
    let nhash = input.read_leb128_u32()? as usize;
    let array = read_array_count(input, narray, |i| lj_tabk(i, big_endian))?;
    let hash = read_array_count(input, nhash, |i| Ok((lj_tabk(i, big_endian)?, lj_tabk(i, big_endian)?) as _))?;
    Ok(LuaConstant::Table(ConstTable { array, hash }.into()))
}

fn combine_number(lo: u32, hi: u32, big_endian: bool) -> f64 {
    unsafe {
        core::mem::transmute(if big_endian {
            ((lo as u64) << 32) | hi as u64
        } else {
            ((hi as u64) << 32) | lo as u64
        })
    }
}

pub fn lj_tabk<'a>(input: & mut Cursor<&[u8]>, big_endian: bool) -> Result<LuaConstant> {
    let ty = input.read_leb128_unsigned(0)? as usize;

    match ty {
        BCDUMP_KTAB_NIL => Ok(LuaConstant::Null),
        BCDUMP_KTAB_FALSE => Ok(LuaConstant::Bool(false)),
        BCDUMP_KTAB_TRUE => Ok(LuaConstant::Bool(true)),
        BCDUMP_KTAB_INT => Ok(LuaConstant::Number(LuaNumber::Integer(input.read_leb128_u32()? as _))),
        BCDUMP_KTAB_NUM => {
            let lo = input.read_leb128_u32()?;
            let hi = input.read_leb128_u32()?;
            Ok(LuaConstant::Number(LuaNumber::Float(combine_number(lo, hi, big_endian))))
        },
        _ if ty >= BCDUMP_KTAB_STR as usize => {
            let len = ty - BCDUMP_KTAB_STR;
            Ok( LuaConstant::String(input.read_bytes(len as u64)?))
        }
        _ => unreachable!("BCDUMP_KTAB: {ty}"),
    }
}

fn lj_num_constant<'a>(
    input: & mut Cursor<&[u8]>,
    big_endian: bool,
) -> Result<LuaNumber> {
    let isnum = input.read_u8()? & 1 != 0;
    let lo = input.read_leb128_unsigned(0)? as u32;
    if isnum {
        let hi = input.read_leb128_u32()?;
        Ok(LuaNumber::Float(combine_number(lo, hi, big_endian)))
    } else {
        Ok(LuaNumber::Integer(lo as i32 as _))
    }
}

fn lj_proto<'a, 'h>(
    header: &'h LuaHeader,
    input: & mut Cursor<&[u8]>,
    stack: &'h mut VecDeque<LuaChunk>,
) -> Result<Option<LuaChunk>> {
    let size = input.read_leb128_u32()?;
    if size == 0 {
        return Ok(None);
    }
    let flags = input.read_u8()?;
    let num_params = input.read_u8()?;
    let framesize = input.read_u8()?;
    let num_upvalues = input.read_u8()?;
    let complex_constants_count = input.read_leb128_u32()?;
    let numeric_constants_count= input.read_leb128_u32()?;
    let instructions_count = input.read_leb128_u32()?;

    let mut line_defined = 0;
    let mut numline = 0;
    let mut debuginfo_size = 0;
    if !has_luajit_flag(header, FLAG_IS_STRIPPED) {
        debuginfo_size = input.read_leb128_u64()?;
    }
    if debuginfo_size > 0 {
        line_defined = input.read_leb128_u64()?;
        numline = input.read_leb128_u64()?;
    }
    let last_line_defined = line_defined + numline;


    let mut protos = vec![];

    let instructions = read_array_count(input, instructions_count as usize, |i| {
        if header.big_endian {
            return i.read_u32_be();
        }
        i.read_u32_le()
    })?;
    let upvalue_infos= read_array_count(input, num_upvalues as usize, |i| {
        let v = if header.big_endian { i.read_u16_be()?} else {i.read_u16_le()?};
        Ok(UpVal {
            on_stack: v & 0x8000 != 0,
            id: (v & 0x7FFF) as _,
            kind: 0,
        })
    })?;
    let mut constants= read_array_count(input, complex_constants_count as usize, |i| {
        lj_complex_constant(stack, &mut protos, i, header.big_endian)
    })?;
    let num_constants= read_array_count(input, numeric_constants_count as usize, |i| {
        lj_num_constant(i, header.big_endian)
    })?;
    
    constants.reverse();

    if debuginfo_size > 0 {
        // input.seek_relative(debuginfo_size as i64)?;
        let line_infos = read_array_count(input, instructions_count as usize, |i| {
            let mut offset: u32;
            if numline >= 65536 {
                offset = i.read_u32_le()?;
            } else if numline >= 256 {
                offset = i.read_u16_le()? as _;
            } else {
                offset = i.read_u8()? as _;
            }
            Ok(line_defined as u32 + offset)
        })?;
        let names = read_array_count(input, num_upvalues as usize, |i| {
            i.read_string_zero_term()
        })?;
        let mut last_addr = 0;
        loop {
            let internal_vartype = input.read_u8()?;
            if internal_vartype == VARNAME_END {
                break;
            }
            if internal_vartype >= VARNAME_MAX {
                let addr_name = input.read_string_zero_term()?;
            }
            let start_addr = last_addr + input.read_leb128_unsigned(0)?;
            let end_addr = start_addr + input.read_leb128_unsigned(0)?;
            last_addr = start_addr;
        }
    }
    Ok(
        Some(LuaChunk {
            name: vec![],
            num_upvalues,
            num_params,
            line_defined,
            last_line_defined,
            flags,
            instructions,
            upvalue_infos,
            constants,
            num_constants,
            max_stack: framesize,
            is_vararg: if flags & PROTO_VARARG != 0 {
                Some(LuaVarArgInfo::new())
            } else {
                None
            },
            prototypes: protos,
            ..Default::default()
        })
    )
}

pub fn lj_chunk<'h, 'a: 'h>(
    header: &'h LuaHeader,
    input: & mut Cursor<&[u8]>
) -> Result<LuaChunk> {
    let mut name = vec![0;0];
    if !has_luajit_flag(header, FLAG_IS_STRIPPED) {
        let namelen = input.read_leb128_u32()?;
        name = input.read_bytes(namelen as u64)?;
    }
    let mut protos: VecDeque<LuaChunk> = VecDeque::new();

    while let Some(proto) = lj_proto(&header, input, &mut protos)? {
        protos.push_back(proto);
    }
    if let Some(mut chunk) = protos.pop_front() {
        chunk.name = name.to_vec();
        Ok(chunk)
    } else {
        Err(Error::form_str("stack unbalanced"))
    }
}

bitflags::bitflags! {
    #[derive(Clone, Copy, Debug, PartialEq, Eq, Hash)]
    pub struct ProtoFlags: u8 {
        const HAS_CHILD = 0b00000001;
        const IS_VARIADIC = 0b00000010;
        const HAS_FFI = 0b00000100;
        const JIT_DISABLED = 0b00001000;
        const HAS_ILOOP = 0b0001000;
    }
}
