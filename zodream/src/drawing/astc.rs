use byteorder::{BigEndian, WriteBytesExt};
use texture2ddecoder::decode_astc;

use super::{PixelDecoder, Result};

pub struct AstcDecoder
{
    block_width: usize,
    block_height: usize
}

impl AstcDecoder 
{
    pub fn new(block_width: usize, block_height: usize) -> AstcDecoder 
    {
        AstcDecoder{block_width, block_height}
    }
}

impl PixelDecoder for AstcDecoder 
{
    fn decode(&mut self, input: &[u8], width: u32, height: u32, output: &mut Vec<u8>) -> Result<usize> {
        let mut buffer = Vec::new();
        decode_astc(input, width as usize, height as usize, self.block_width, self.block_height, &mut buffer)?;
        for i in buffer {
            output.write_u32::<BigEndian>(i).unwrap();
        }
        Ok(output.len())
    }
}