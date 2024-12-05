use std::{borrow::Cow, io::{Cursor, Seek}};
use crate::{error::{Error, Result}, io::ByteReadExt};

pub mod lua51;
pub mod lua52;
pub mod lua53;
pub mod lua54;
pub mod luajit;
pub mod luau;
pub mod utils;

#[derive(Clone, Default, Debug, PartialEq, Eq)]
pub struct LuaHeader {
    pub lua_version: u8,
    pub format_version: u8,
    pub big_endian: bool,
    pub int_size: u8,
    pub size_t_size: u8,
    pub instruction_size: u8,
    pub number_size: u8,
    pub number_integral: bool,
    // for luajit
    pub lj_flags: u8,
}

impl LuaHeader {
    pub fn version(&self) -> LuaVersion {
        LuaVersion(self.lua_version)
    }
}

#[derive(Debug, Clone, Copy, PartialEq)]
pub enum LuaNumber {
    Integer(i64),
    Float(f64),
}

impl std::fmt::Display for LuaNumber {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::Float(n) => write!(f, "{n}"),
            Self::Integer(i) => write!(f, "{i}"),
        }
    }
}

/// Constant table for luajit, notice that the index of the array part starts at 0
#[derive(Clone, Debug, Default)]
pub struct ConstTable {
    pub array: Vec<LuaConstant>,
    pub hash: Vec<(LuaConstant, LuaConstant)>,
}

#[derive(Clone, Default)]
pub enum LuaConstant {
    #[default]
    Null,
    Bool(bool),
    Number(LuaNumber),
    String(Vec<u8>),
    // for luajit
    Proto(usize),
    Table(Box<ConstTable>),
    // // for luau
    // Imp(u32),
}

impl std::fmt::Debug for LuaConstant {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match self {
            Self::Null => write!(f, "Null"),
            Self::Bool(arg0) => f.debug_tuple("Bool").field(arg0).finish(),
            Self::Number(arg0) => match arg0 {
                LuaNumber::Float(n) => f.debug_tuple("Number").field(n).finish(),
                LuaNumber::Integer(n) => f.debug_tuple("Integer").field(n).finish(),
            },
            Self::String(arg0) => f
                .debug_tuple("String")
                .field(&String::from_utf8_lossy(arg0))
                .finish(),
            Self::Proto(i) => f.debug_tuple("Proto").field(i).finish(),
            Self::Table(b) => f
                .debug_struct("Table")
                .field("array", &b.array)
                .field("hash", &b.hash)
                .finish(),
            // Self::Imp(imp) => f.debug_tuple("Imp").field(imp).finish(),
        }
    }
}

#[derive(Debug, Default)]
pub struct LuaLocal {
    pub name: String,
    pub start_pc: u64,
    pub end_pc: u64,
    pub reg: u8, // for luau
}

#[derive(Debug)]
pub struct LuaVarArgInfo {
    pub has_arg: bool,
    pub needs_arg: bool,
}

impl LuaVarArgInfo {
    pub fn new() -> Self {
        Self {
            has_arg: true,
            needs_arg: true,
        }
    }
}

#[derive(Debug, Default)]
pub struct UpVal {
    pub on_stack: bool,
    pub id: u8,
    pub kind: u8,
}

#[derive(Default)]
pub struct LuaChunk {
    pub name: Vec<u8>,
    // 第一行的行号
    pub line_defined: u64,
    // 最后一行的行号
    pub last_line_defined: u64,
    pub num_upvalues: u8,
    pub num_params: u8,
    /// Equivalent to framesize for luajit
    pub max_stack: u8,
    /// for luajit
    pub flags: u8,
    pub is_vararg: Option<LuaVarArgInfo>,
    pub instructions: Vec<u32>,
    pub constants: Vec<LuaConstant>,
    /// for luajit
    pub num_constants: Vec<LuaNumber>,
    pub prototypes: Vec<Self>,
    pub source_lines: Vec<(u32, u32)>,
    pub locals: Vec<LuaLocal>,
    /// for lua53
    pub upvalue_infos: Vec<UpVal>,
    pub upvalue_names: Vec<Vec<u8>>,
}

impl std::fmt::Debug for LuaChunk {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("LuaChunk")
            .field("name", &String::from_utf8_lossy(&self.name))
            .field("line_defined", &self.line_defined)
            .field("last_line_defined", &self.last_line_defined)
            .field("is_vararg", &self.is_vararg.is_some())
            .field("num_params", &self.num_params)
            .field("num_upvalues", &self.num_upvalues)
            .field("locals", &self.locals)
            .field("constants", &self.constants)
            .field("prototypes", &self.prototypes)
            .field("upvalue_infos", &self.upvalue_infos)
            .finish()
    }
}

impl LuaChunk {
    pub fn name(&self) -> Cow<str> {
        String::from_utf8_lossy(&self.name)
    }

    pub fn flags(&self) -> luajit::ProtoFlags {
        luajit::ProtoFlags::from_bits(self.flags).unwrap()
    }

    pub fn is_empty(&self) -> bool {
        self.instructions.is_empty()
    }
}

#[derive(Debug)]
pub struct LuaBytecode {
    pub header: LuaHeader,
    pub main_chunk: LuaChunk,
}

fn lua_header(input: & mut Cursor<&[u8]>) -> Result<LuaHeader> {
    let magic = input.read_bytes(4)?;
    if magic != b"\x1BLua" {
        return Err(Error::form_str("lua magic error"));
    }
    let version = input.read_u8()?;
    match version  {
        0x51 => Ok(
            LuaHeader {
                lua_version: LUA51.0,
                format_version: input.read_u8()?,
                big_endian: input.read_u8()? != 1,
                int_size: input.read_u8()?,
                size_t_size: input.read_u8()?,
                instruction_size: input.read_u8()?,
                number_size: input.read_u8()?,
                number_integral: input.read_u8()? != 0,
                ..Default::default()
            }
        ),
        0x52 => {
            let r = LuaHeader {
                lua_version: LUA52.0,
                format_version: input.read_u8()?,
                big_endian: input.read_u8()? != 1,
                int_size: input.read_u8()?,
                size_t_size: input.read_u8()?,
                instruction_size: input.read_u8()?,
                number_size: input.read_u8()?,
                number_integral: input.read_u8()? != 0,
                ..Default::default()
            };
            input.seek_relative(6)?; // LUAC_DATA
            Ok(r)
        },
        0x53 => {
            let format_version = input.read_u8()?;
            input.seek_relative(6)?;// LUAC_DATA
            let int_size = input.read_u8()?;
            let size_t_size = input.read_u8()?;
            let instruction_size = input.read_u8()?;
            input.seek_relative(1)?; // lua_Integer
            let number_size  = input.read_u8()?;
            input.seek_relative(17)?;
            Ok(
                LuaHeader {
                    lua_version: LUA53.0,
                    format_version,
                    big_endian: true,
                    int_size,
                    size_t_size,
                    instruction_size,
                    number_size,
                    number_integral: false,
                    ..Default::default()
                }
            )
        },
        0x54 => {
            let format_version = input.read_u8()?;
            input.seek_relative(6)?;// LUAC_DATA
            let instruction_size = input.read_u8()?;
            input.seek_relative(1)?; // lua_Integer
            let number_size  = input.read_u8()?;
            input.seek_relative(17)?;
            Ok(
                LuaHeader {
                    lua_version: LUA53.0,
                    format_version,
                    big_endian: true,
                    instruction_size,
                    number_size,
                    number_integral: false,
                    ..Default::default()
                }
            )
        }
        _ => Err(Error::form_str("version error"))
    }
}


fn lua_int_flags(header: &LuaHeader, input: & mut Cursor<&[u8]>, flags: u8) -> Result<u64> {
    match flags {
        8 => {
            if header.big_endian {
                return input.read_u64_be()
            }
            input.read_u64_le()
        },
        4 => {
            if header.big_endian {
                return Ok(input.read_u32_be()? as u64)
            }
            Ok(input.read_u32_le()? as u64)
        },
        2 => {
            if header.big_endian {
                return Ok(input.read_u16_be()? as u64)
            }
            Ok(input.read_u16_le()? as u64)
        },
        1 => Ok(input.read_u8()? as u64),
        _ => Ok(0)
    }
}


fn lua_int(header: &LuaHeader, input: & mut Cursor<&[u8]>) -> Result<u64> {
    lua_int_flags(header, input, header.int_size)
}

fn lua_size_t(header: &LuaHeader, input: & mut Cursor<&[u8]>) -> Result<u64> {
    lua_int_flags(header, input, header.size_t_size)
}

fn lua_number(header: &LuaHeader, input: & mut Cursor<&[u8]>) -> Result<LuaNumber> {
    if header.number_integral {
        match header.number_size {
            8 => {
                if header.big_endian {
                    return Ok(LuaNumber::Integer(input.read_i64_be()?))
                }
                Ok(LuaNumber::Integer(input.read_i64_le()?))
            },
            4 => {
                if header.big_endian {
                    return Ok(LuaNumber::Integer(input.read_i32_be()? as i64))
                }
                Ok(LuaNumber::Integer(input.read_i32_le()? as i64))
            },
            2 => {
                if header.big_endian {
                    return Ok(LuaNumber::Integer(input.read_i16_be()? as i64))
                }
                Ok(LuaNumber::Integer(input.read_i16_le()? as i64))
            },
            1 => Ok(LuaNumber::Integer(input.read_u8()? as i64)),
            _ => Ok(LuaNumber::Integer(0))
        }
    } else {
        match header.number_size {
            8 => {
                if header.big_endian {
                    return Ok(LuaNumber::Float(input.read_f64_be()?))
                }
                Ok(LuaNumber::Float(input.read_f64_le()?))
            },
            4 => {
                if header.big_endian {
                    return Ok(LuaNumber::Float(input.read_f32_be()? as f64))
                }
                Ok(LuaNumber::Float(input.read_f32_le()? as f64))
            },
            _ => Ok(LuaNumber::Float(0f64))
        }
    }
    
}

pub fn lua_bytecode(input: & mut Cursor<&[u8]>) -> Result<LuaBytecode> {
    let header = match lua_header(input) {
        Ok(h) => h,
        Err(_) => {
            input.set_position(0);
            luajit::lj_header(input)?
        }
    };
    let main_chunk = match header.version() {
        LUA51 => lua51::lua_chunk(&header, input),
        LUA52 => lua52::lua_chunk(&header, input),
        LUA53 => lua53::lua_chunk(&header, input),
        LUA54 => lua54::lua_chunk(&header, input),
        LUAJ1 | LUAJ2 => luajit::lj_chunk(&header, input),
        _ => Err(Error::form_str("unsupported lua version")),
    }?;
    Ok(LuaBytecode { header, main_chunk })
}

pub fn parse(input: &[u8]) -> Result<LuaBytecode> {
    let mut cur = Cursor::new(input);
    lua_bytecode(&mut cur)
}


#[derive(Debug, Clone, Copy, PartialEq, Eq, PartialOrd, Ord)]
pub struct LuaVersion(pub u8);

impl std::fmt::Display for LuaVersion {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        match *self {
            LUAJ1 => write!(f, "luajit1"),
            LUAJ2 => write!(f, "luajit2"),
            v => write!(f, "lua{:x}", v.0),
        }
    }
}

impl LuaVersion {
    pub fn is_luajit(self) -> bool {
        matches!(self, LUAJ1 | LUAJ2)
    }
}

pub const LUA51: LuaVersion = LuaVersion(0x51);
pub const LUA52: LuaVersion = LuaVersion(0x52);
pub const LUA53: LuaVersion = LuaVersion(0x53);
pub const LUA54: LuaVersion = LuaVersion(0x54);
pub const LUAJ1: LuaVersion = LuaVersion(0x11);
pub const LUAJ2: LuaVersion = LuaVersion(0x12);
