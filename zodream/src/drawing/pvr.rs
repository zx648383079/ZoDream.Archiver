use texture2ddecoder::decode_pvrtc;

use super::{color::ColorWriteExt, PixelDecoder, Result};

pub struct PvrDecoder
{
    version: u8,
}

impl PvrDecoder 
{
    pub fn new(version: u8) -> PvrDecoder 
    {
        PvrDecoder{version}
    }
}

impl PixelDecoder for PvrDecoder 
{
    fn decode(&mut self, input: &[u8], width: u32, height: u32, output: &mut Vec<u8>) -> Result<usize> {
        let mut buffer = Vec::new();
        decode_pvrtc(input, width as usize, height as usize, &mut buffer, self.version == 2)?;
        for i in buffer {
            output.write_bgra(i)?;
        }
        Ok(output.len())
    }
}