use byteorder::{BigEndian, WriteBytesExt};
use texture2ddecoder::decode_crunch;

use super::{PixelDecoder, Result};

pub struct CrunchDecoder
{
}

impl CrunchDecoder 
{
    pub fn new() -> CrunchDecoder 
    {
        CrunchDecoder{}
    }
}

impl PixelDecoder for CrunchDecoder 
{
    fn decode(&mut self, input: &[u8], width: u32, height: u32, output: &mut Vec<u8>) -> Result<usize> {
        let mut buffer = Vec::new();
        decode_crunch(input, width as usize, height as usize, &mut buffer)?;
        for i in buffer {
            output.write_u32::<BigEndian>(i).unwrap();
        }
        Ok(output.len())
    }
}