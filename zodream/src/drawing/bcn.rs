use texture2ddecoder::{decode_bc1, decode_bc3, decode_bc4, decode_bc5, decode_bc6, decode_bc7};

use crate::io::ByteWriteExt;

use super::{PixelDecoder, Result};

pub struct BcnDecoder
{
    version: u8
}

impl BcnDecoder 
{
    pub fn new(version: u8) -> BcnDecoder 
    {
        BcnDecoder{version}
    }
}

impl PixelDecoder for BcnDecoder 
{
    fn decode(&mut self, input: &[u8], width: u32, height: u32, output: &mut Vec<u8>) -> Result<usize> {
        let mut buffer = Vec::new();
        match self.version {
            1 => decode_bc1(input, width as usize, height as usize, &mut buffer)?,
            3 => decode_bc3(input, width as usize, height as usize, &mut buffer)?,
            4 => decode_bc4(input, width as usize, height as usize, &mut buffer)?,
            5 => decode_bc5(input, width as usize, height as usize, &mut buffer)?,
            6 => decode_bc6(input, width as usize, height as usize, &mut buffer, false)?,
            7 => decode_bc7(input, width as usize, height as usize, &mut buffer)?,
            _ => ()
        }
        for i in buffer {
            output.write_u32_be(i).unwrap();
        }
        Ok(output.len())
    }
}