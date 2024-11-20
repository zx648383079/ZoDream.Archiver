use byteorder::{BigEndian, WriteBytesExt};
use texture2ddecoder::decode_etc1;

use super::{PixelDecoder, Result};

pub struct EtcDecoder
{
}

impl EtcDecoder 
{
    pub fn new() -> EtcDecoder 
    {
        EtcDecoder{}
    }
}

impl PixelDecoder for EtcDecoder 
{
    fn decode(&mut self, input: &[u8], width: u32, height: u32, output: &mut Vec<u8>) -> Result<usize> {
        let mut buffer = Vec::new();
        decode_etc1(input, width as usize, height as usize, &mut buffer)?;
        for i in buffer {
            output.write_u32::<BigEndian>(i).unwrap();
        }
        Ok(output.len())
    }
}