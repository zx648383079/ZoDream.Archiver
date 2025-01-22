use texture2ddecoder::{decode_atc_rgb4, decode_atc_rgba8};

use super::{color::ColorWriteExt, PixelDecoder, Result};

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
        let len = width * height;
        let mut buffer = vec![0u32; len as usize];
        if self.version == 8 {
            decode_atc_rgba8(input, width as usize, height as usize, &mut buffer)?;
        } else {
            decode_atc_rgb4(input, width as usize, height as usize, &mut buffer)?;
        }
        for i in buffer {
            output.write_bgra(i)?;
        }
        Ok(output.len())
    }
}