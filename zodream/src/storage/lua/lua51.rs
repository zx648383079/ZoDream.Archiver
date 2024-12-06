use luau::read_array_count;

use super::*;

pub fn lua_string<'a>(header: &LuaHeader, input: & mut Cursor<&[u8]>) -> Result<Vec<u8>> {
    let count = lua_size_t(header, input)?;
    if count < 1 {
        return Ok(vec![0;0]);
    }
    let res =  input.read_bytes(count - 1)?;
    input.seek_relative(1)?; // 去除结尾的 0x0
    Ok(res)
}

pub fn lua_local<'a>(header: &LuaHeader, input: & mut Cursor<&[u8]>) -> Result<LuaLocal> {
    let name = lua_string(header, input)?;
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
    let name = lua_string(header, input)?;
    let line_defined = lua_int(header, input)?;
    let last_line_defined = lua_int(header, input)?;
    let num_upvalues = input.read_u8()?; 
    let num_params= input.read_u8()?;
    let is_vararg = input.read_u8()?;
    let max_stack= input.read_u8()?;

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
            1 => Ok(LuaConstant::Bool(i.read_u8()? != 0)),
            3 => Ok(LuaConstant::Number(lua_number(header, i)?)),
            4 => Ok(LuaConstant::String(lua_string(header, i)?)),
            _ => Err(Error::form_str("count constants error"))
        }
    })?;
    let num =  lua_int(header, input)? as usize;
    let prototypes = read_array_count(input, num, |i| {
        lua_chunk(header, i)
    })?;
    let num = lua_int(header, input)? as usize;
    let source_lines = read_array_count(input, num, |i| {
        Ok((lua_int(header, i)? as u32, 0u32))
    })?;
    let num = lua_int(header, input)? as usize;
    let locals = read_array_count(input, num, |i| {
        lua_local(header, i)
    })?;
    let num = lua_int(header, input)? as usize;
    let upvalue_names =  read_array_count(input, num, |i| {
        lua_string(header, i)
    })?;
    Ok(LuaChunk {
        name: name.to_vec(),
        line_defined,
        last_line_defined,
        num_upvalues,
        num_params,
        flags: 0,
        is_vararg: if (is_vararg & 2) != 0 {
            Some(LuaVarArgInfo {
                has_arg: (is_vararg & 1) != 0,
                needs_arg: (is_vararg & 4) != 0,
            })
        } else {
            None
        },
        max_stack,
        instructions,
        constants,
        num_constants: vec![],
        prototypes,
        source_lines,
        locals,
        upvalue_names,
        upvalue_infos: vec![],
    })
}
