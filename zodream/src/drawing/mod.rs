use std::io::{Read, Write};
use super::error::Result;

pub mod atc;

pub trait PixelEncoder {
    fn encode(&mut self, input: &[u8], width: u32, height: u32, output: &mut Vec<u8>) -> Result<usize>;
}

pub fn encode_stream<T, R, W>(target: & mut T, input: &mut R, width: u32, height: u32, output: &mut  W) -> Result<usize>
    where R : Read, W : Write, T : PixelEncoder + ?Sized
{
    let mut in_buf = Vec::new();
    let mut out_buf = Vec::new();
    input.read_to_end(&mut in_buf);
    let len = target.encode(&in_buf, width, height, &mut out_buf)?;
    output.write(&out_buf[..len]);
    Ok(len)
}

pub trait PixelDecoder {
    fn decode(&mut self, input: &[u8], width: u32, height: u32, output: &mut Vec<u8>) -> Result<usize>;
}

pub fn decode_stream<T, R, W>(target: & mut T, input: &mut R, width: u32, height: u32, output: &mut  W) -> Result<usize>
    where R : Read, W : Write, T : PixelDecoder + ?Sized
{
    let mut in_buf = Vec::new();
    let mut out_buf = Vec::new();
    input.read_to_end(&mut in_buf);
    let len = target.decode(&in_buf, width, height, &mut out_buf)?;
    output.write(&out_buf[..len]);
    Ok(len)
}

