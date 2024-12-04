use std::io::{Read, Write};

use crate::storage::lua::parse;
use super::{Decompressor, Result};


pub struct LuaCompressor
{

}

impl LuaCompressor {
    pub fn new() -> LuaCompressor
    {
        LuaCompressor{}
    }
}

impl Decompressor for LuaCompressor
{
    fn decompress<R: Read, W: Write>(&mut self, input: &mut R, output: &mut W) -> Result<usize>
    {
        let mut in_buf = Vec::new();
        // let mut out_buf = Vec::new();
        input.read_to_end(&mut in_buf)?;
        let res = parse(&in_buf)?;
        Ok(0)
    }
}