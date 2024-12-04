use luau::read_array_count;

use super::{
    lua51::{lua_local, lua_string},
    *,
};

pub fn load_upvalue(input: & mut Cursor<&[u8]>) -> Result<UpVal> {
    let on_stack = input.read_u8()?;
    let id = input.read_u8()?;

    Ok(UpVal {
        on_stack: on_stack != 0,
        id,
        kind: 0,
    })
}

pub fn lua_chunk<'h, 'a: 'h>(
    header: &'h LuaHeader,
    input: & mut Cursor<&[u8]>
) -> Result<LuaChunk> {
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
            1 => Ok(LuaConstant::Bool(i.read_u8()? != 0)),
            3 => Ok(LuaConstant::Number(lua_number(header, i)?)),
            4 => Ok(LuaConstant::String(lua_string(header, i)?)),
            _ => Err(Error::form_str("count constants error"))
        }
    })?;
    let num = lua_int(header, input)? as usize;
    let prototypes = read_array_count(input, num, |i| {
        lua_chunk(header, i)
    })?;
    let num = lua_int(header, input)? as usize;
    let upvalue_infos = read_array_count(input, num, |i| {
        load_upvalue(i)
    })?;
    let name = lua_string(header, input)?;
    let num = lua_int(header, input)? as usize;
    let source_lines = read_array_count(input, num, |i| {
        Ok((lua_int(header, i)? as u32, 0u32))
    })?;
    let num = lua_int(header, input)? as usize;
    let locals = read_array_count(input, num, |i| {
        lua_local(header, i)
    })?;
    let num = lua_int(header, input)? as usize;
    let upvalue_names=  read_array_count(input, num, |i| {
        lua_string(header, i)
    })?;
    Ok(LuaChunk {
        name,
        line_defined,
        last_line_defined,
        num_upvalues: upvalue_infos.len() as _,
        num_params,
        flags: 0,
        is_vararg: if is_vararg != 0 {
            Some(LuaVarArgInfo::new())
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
        upvalue_infos,
    })
}
