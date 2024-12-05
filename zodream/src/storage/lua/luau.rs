use super::*;


pub fn read_string_ref<'a>(input: & mut Cursor<&[u8]>, stable: &[Vec<u8>]) -> Result<Vec<u8>> {
    let i = input.read_leb128_unsigned(0)? as usize;
    if i > 0 {
        Ok(stable[i - 1].clone())
    } else {
        Ok(vec![0;0])
    } 
}

pub const LBC_CONSTANT_NIL: u8 = 0;
pub const LBC_CONSTANT_BOOLEAN: u8 = 1;
pub const LBC_CONSTANT_NUMBER: u8 = 2;
pub const LBC_CONSTANT_STRING: u8 = 3;
pub const LBC_CONSTANT_IMPORT: u8 = 4;
pub const LBC_CONSTANT_TABLE: u8 = 5;
pub const LBC_CONSTANT_CLOSURE: u8 = 6;

pub fn read_table<'a>(input: & mut Cursor<&[u8]>, k: &[LuaConstant]) -> Result<ConstTable> {
    let numk = input.read_leb128_unsigned(0)? as usize;
    let mut result = ConstTable {
        hash: Vec::with_capacity(numk),
        ..Default::default()
    };
    for _ in 0..numk {
        let ik = input.read_leb128_unsigned(0)? as usize;
        result
            .hash
            .push((k[ik].clone(), LuaConstant::Number(LuaNumber::Integer(0))));
    }
    Ok(result)
}

pub fn read_array<'a, T, F>(input: & mut Cursor<&[u8]>, cb: F) -> Result<Vec<T>> 
    where F : Fn(& mut Cursor<&[u8]>) -> Result<T> {
   let num = input.read_leb128_unsigned(0)? as usize;
   read_array_count(input, num, cb)
}

pub fn read_array_count<'a, T, F>(input: & mut Cursor<&[u8]>, count: usize, cb: F) -> Result<Vec<T>>
    where F : Fn(& mut Cursor<&[u8]>) -> Result<T> {
    let mut items = Vec::with_capacity(count);
    for _ in 0..count {
        items.push(cb(input)?);
    }
    Ok(items)
 }

pub fn constants<'a>(
    input: & mut Cursor<&[u8]>,
    stable: &[Vec<u8>],
) -> Result<Vec<LuaConstant>> {
    let num  = input.read_leb128_unsigned(0)? as usize;
    let mut result = Vec::with_capacity(num);
    for _ in 0..num {
        let ty = input.read_u8()?;
        let k = match ty {
            LBC_CONSTANT_NIL => LuaConstant::Null,
            LBC_CONSTANT_BOOLEAN => LuaConstant::Bool(input.read_u8()? != 0),
            LBC_CONSTANT_NUMBER => LuaConstant::Number(LuaNumber::Float(input.read_f64_le()?)),
            LBC_CONSTANT_STRING => {
                LuaConstant::String(read_string_ref(input, stable)?.into())
            },
            // LBC_CONSTANT_IMPORT => map(complete::be_u32, |i| LuaConstant::Imp(i as _))(input),
            LBC_CONSTANT_IMPORT => {
                input.seek_relative(4)?;
                LuaConstant::Null
            },
            LBC_CONSTANT_TABLE => {
                let t = read_table(input, &result)?;
                LuaConstant::Table(t.into())
            }
            LBC_CONSTANT_CLOSURE => {
                LuaConstant::Proto(input.read_leb128_unsigned(0)? as usize)
            },
            // _ => context("string", fail::<&u8, LuaConstant, _>).parse(input),
            _ => unreachable!("const type: {ty}"),
        };
        result.push(k);
    }
    Ok(result)
}

pub fn bytecode(input: & mut Cursor<&[u8]>) -> Result<LuaChunk> {
    let _version = input.read_u8()?;
    let mut types_version = 0;

    if _version >= 4 {
        let _types_version = input.read_u8()?;
        types_version = _types_version;
    }

    // string table
    let stable = read_array(input, |i| {
        let n = i.read_leb128_unsigned(0)?;
        Ok(i.read_bytes(n)?)
    })?;
    

    // proto table
    let num = input.read_leb128_unsigned(0)? as usize;
    let mut protos = Vec::with_capacity(num);

    for _ in 0..num {
        let max_stack = input.read_u8()?;
        let num_params= input.read_u8()?; 
        let num_upvalues= input.read_u8()?;
        let is_vararg = input.read_u8()?;

        if _version >= 4 {
            let _flags = input.read_u8()?;
            let types_size = input.read_leb128_unsigned(0)?;

            if types_size > 0 && types_version == 1 {
                for _ in 0..types_size {
                    let _byte = input.read_u8()?;
                }
            }
        }

        let instructions: Vec<u32> = read_array(input, |i | {
            i.read_u32_le()
        })?;
        let constants = constants(input, stable.as_slice())?;

        let num = input.read_leb128_unsigned(0)? as usize;
        let mut prototypes = Vec::with_capacity(num);
        for _ in 0..num {
            let n = input.read_leb128_unsigned(0)? as usize;
            prototypes.push(core::mem::take(&mut protos[n]));
        }
        let line_defined = input.read_leb128_unsigned(0)?;
        let name = read_string_ref(input, &stable)?;
        let has_lineinfo = input.read_u8()?;

        if has_lineinfo > 0 {
            let linegaplog2 = input.read_u8()?;
            let intervals = ((instructions.len() - 1) >> (linegaplog2 as usize)) + 1;
            let _lineinfo = read_array_count(input, instructions.len(), |i| Ok(i.read_u8()))?;
            let _abslineinfo = read_array_count(input, intervals, |i| i.read_i32_be())?;
        }

        let has_debuginfo = input.read_u8()?;
        let mut locals = vec![];
        let mut upvalue_names = vec![];
        if has_debuginfo > 0 {
            locals = read_array(input, |i| {
                let name = read_string_ref(i, &stable)?;
                let start = i.read_leb128_unsigned(0)?;
                let end = i.read_leb128_unsigned(0)?;
                let reg = i.read_u8()?;
                Ok(LuaLocal {
                    name: String::from_utf8_lossy(name.as_slice()).into(),
                    start_pc: start,
                    end_pc: end,
                    reg,
                })
            })?;
            upvalue_names = read_array(input, |i| {
                read_string_ref(i, &stable)
            })?;
        }
        let proto = LuaChunk {
            name: name.to_vec(),
            line_defined,
            last_line_defined: 0,
            num_upvalues,
            num_params,
            max_stack,
            prototypes,
            is_vararg: if is_vararg > 0 {
                Some(LuaVarArgInfo {
                    has_arg: true,
                    needs_arg: true,
                })
            } else {
                None
            },
            instructions,
            constants,
            locals,
            upvalue_names,
            ..Default::default()
        };
        protos.push(proto);
    }

    let mainid = input.read_leb128_unsigned(0)? as usize;
    let main = core::mem::take(&mut protos[mainid]);
    assert!(!main.is_empty());

    Ok(main)
}
