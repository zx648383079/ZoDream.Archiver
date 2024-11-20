use byteorder::{BigEndian, WriteBytesExt};
use texture2ddecoder::decode_pvrtc;

use super::{PixelDecoder, Result};

pub struct PvrDecoder
{
}

impl PvrDecoder 
{
    pub fn new() -> PvrDecoder 
    {
        PvrDecoder{}
    }
}

impl PixelDecoder for PvrDecoder 
{
    fn decode(&mut self, input: &[u8], width: u32, height: u32, output: &mut Vec<u8>) -> Result<usize> {
        let mut buffer = Vec::new();
        decode_pvrtc(input, width as usize, height as usize, &mut buffer, true)?;
        for i in buffer {
            output.write_u32::<BigEndian>(i).unwrap();
        }
        Ok(output.len())
    }
}