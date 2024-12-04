use luau::read_array_count;

use super::*;

pub fn load_string(input: & mut Cursor<&[u8]>) -> Result<Vec<u8>> {
    let n = input.read_u8()? as u64;
    if n == 0 {
        return Ok(vec![0]);
    }
    input.read_bytes(n - 1)
}

pub fn load_upvalue(input: & mut Cursor<&[u8]>) -> Result<UpVal> {
    let on_stack = input.read_u8()?;
    let id = input.read_u8()?;
    let kind = input.read_u8()?;
    Ok(UpVal {
        on_stack: on_stack != 0,
        id,
        kind,
    })
}

pub fn lua_local<'a>(header: &LuaHeader, input: & mut Cursor<&[u8]>) -> Result<LuaLocal> {
    let name = load_string(input)?;
    let start_pc = lua_int(header, input)?;
    let end_pc = lua_int(header, input)?;

    Ok(LuaLocal {
        name: String::from_utf8_lossy(&name).into(),
        start_pc,
        end_pc,
        ..Default::default()
    })
}

pub fn lua_chunk<'h, 'a: 'h>(
    header: &'h LuaHeader,
    input: & mut Cursor<&[u8]>
) -> Result<LuaChunk> {
    let name = load_string(input)?;
    let line_defined = lua_int(header, input)?;
    let last_line_defined = lua_int(header, input)?;
    let num_params = input.read_u8()?;
    let is_vararg = input.read_u8()?;
    let max_stack = input.read_u8()?;

    let num = lua_int(header, input)? as usize;
    let instructions = read_array_count(input, num, |i| {
        if header.instruction_size == 4 {
            if header.big_endian {
                return i.read_u32_be()
            }
            return i.read_u32_le()
        }
        Ok(0)
    })?;
    let num = lua_int(header, input)? as usize;
    let constants = read_array_count(input, num, |i| {
        let b = i.read_u8()?;
        match b {
            0 => Ok(LuaConstant::Null),
            1 => Ok(LuaConstant::Bool(false)),
            0x11 => Ok(LuaConstant::Bool(true)),
            0x13 => Ok(LuaConstant::Number(LuaNumber::Float(i.read_f64_le()?))),
            4 |  0x14 => {
                Ok(LuaConstant::String(load_string(i)?))
            },
            0x03 => Ok(LuaConstant::Number(LuaNumber::Integer(i.read_u64_le()? as _))),
            _ => Err(Error::form_str("count constants error"))
        }
    })?;
    let num = lua_int(header, input)? as usize;
    let upvalues= read_array_count(input, num, |i| {
        load_upvalue(i)
    })?;
    let num = lua_int(header, input)? as usize;
    let prototypes = read_array_count(input, num, |i| {
        lua_chunk(header, i)
    })?;
    let num = lua_int(header, input)? as usize;
    let _line_info = read_array_count(input, num, |i| {
        i.read_u8()
    })?;
    let num = lua_int(header, input)? as usize;
    let source_lines = read_array_count(input, num, |i| {
        Ok((lua_int(header, i)? as u32, lua_int(header, i)? as u32))
    })?;
    let num = lua_int(header, input)? as usize;
    let locals = read_array_count(input, num, |i| {
        lua_local(header, i)
    })?;
    let num = lua_int(header, input)? as usize;
    let upvalue_names=  read_array_count(input, num, |i| {
        load_string(i)
    })?;

    Ok(LuaChunk {
        name: name.to_vec(),
        line_defined,
        last_line_defined,
        num_upvalues: upvalues.len() as _,
        flags: 0,
        num_params,
        is_vararg: if is_vararg != 0 {
            Some(LuaVarArgInfo::new())
        } else {
            None
        },
        max_stack,
        instructions,
        constants,
        prototypes,
        source_lines,
        locals,
        upvalue_names,
        num_constants: vec![],
        upvalue_infos: upvalues,
    })
}
