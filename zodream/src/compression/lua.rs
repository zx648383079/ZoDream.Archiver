use std::io::{Read, Write};

use luac_parser::parse;

use super::{copy_to, Compressor, Decompressor, Result, Error};


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
        let mut out_buf = Vec::new();
        input.read_to_end(&mut in_buf)?;
        let res = parse(&in_buf);
        match res {
            Ok(r) => {
                
                Ok(0)
            }
            Err(s) => Err(Error::new(s))
        }
    }
}