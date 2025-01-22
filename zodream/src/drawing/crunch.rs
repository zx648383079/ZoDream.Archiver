use texture2ddecoder::{decode_crunch, decode_unity_crunch};

use super::{color::ColorWriteExt, PixelDecoder, Result};

pub struct CrunchDecoder
{
    version: u8
}

impl CrunchDecoder 
{
    pub fn new(version: u8) -> CrunchDecoder 
    {
        CrunchDecoder{version}
    }
}

impl PixelDecoder for CrunchDecoder 
{
    fn decode(&mut self, input: &[u8], width: u32, height: u32, output: &mut Vec<u8>) -> Result<usize> {
        let mut buffer = Vec::new();
        if self.version == 2 {
            decode_unity_crunch(input, width as usize, height as usize, &mut buffer)?;
        } else {
            decode_crunch(input, width as usize, height as usize, &mut buffer)?;
        }
        for i in buffer {
            output.write_bgra(i)?;
        }
        Ok(output.len())
    }
}