use texture2ddecoder::{decode_eacr, decode_eacr_signed, decode_eacrg, decode_eacrg_signed, decode_etc1, decode_etc2_rgb, decode_etc2_rgba1, decode_etc2_rgba8};

use super::{color::ColorWriteExt, PixelDecoder, Result};

pub struct EtcDecoder
{
    version: u8,
    bit_width: u8,
    signed: bool,
}

impl EtcDecoder 
{
    pub fn new(version: u8, bit_width: u8) -> EtcDecoder 
    {
        EtcDecoder{version, bit_width, signed: false}
    }
    pub fn new_signed(version: u8, bit_width: u8, signed: bool) -> EtcDecoder 
    {
        EtcDecoder{version, bit_width, signed}
    }
}

impl PixelDecoder for EtcDecoder 
{
    fn decode(&mut self, input: &[u8], width: u32, height: u32, output: &mut Vec<u8>) -> Result<usize> {
        let len = (width * height) as usize;
        let mut buffer = vec![0; len];
        if self.version < 1 {
            decode_etc1(input, width as usize, height as usize, &mut buffer)?;
        } else if self.version == 2 && self.bit_width == 1 {
            decode_etc2_rgba1(input, width as usize, height as usize, &mut buffer)?;
        } else if self.version == 2 && self.bit_width == 8 {
            decode_etc2_rgba8(input, width as usize, height as usize, &mut buffer)?;
        } else if self.version == 2 && self.bit_width == 4 {
            decode_etc2_rgb(input, width as usize, height as usize, &mut buffer)?;
        } else if self.version == 3 && self.bit_width == 1 && !self.signed {
            decode_eacr(input, width as usize, height as usize, &mut buffer)?;
        } else if self.version == 3 && self.bit_width == 1 && self.signed {
            decode_eacr_signed(input, width as usize, height as usize, &mut buffer)?;
        } else if self.version == 3 && self.bit_width == 2 && !self.signed {
            decode_eacrg(input, width as usize, height as usize, &mut buffer)?;
        } else if self.version == 3 && self.bit_width == 2 && self.signed {
            decode_eacrg_signed(input, width as usize, height as usize, &mut buffer)?;
        }
        for i in buffer {
            output.write_bgra(i)?;
        }
        Ok(output.len())
    }
}