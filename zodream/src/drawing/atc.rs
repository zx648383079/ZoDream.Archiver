use texture2ddecoder::{decode_atc_rgb4, decode_atc_rgba8};

use crate::io::ByteWriteExt;

use super::{PixelDecoder, Result};

pub struct AtcDecoder
{
    version: u8,
}

impl AtcDecoder 
{
    pub fn new(version: u8) -> AtcDecoder 
    {
        AtcDecoder{version}
    }
}

impl PixelDecoder for AtcDecoder 
{
    fn decode(&mut self, input: &[u8], width: u32, height: u32, output: &mut Vec<u8>) -> Result<usize> {
        let mut buffer = Vec::new();
        if self.version == 8 {
            decode_atc_rgba8(input, width as usize, height as usize, &mut buffer)?;
        } else {
            decode_atc_rgb4(input, width as usize, height as usize, &mut buffer)?;
        }
        for i in buffer {
            output.write_u32_be(i).unwrap();
        }
        Ok(output.len())
    }
}