use std::io::{Read, Write};

use lz4::EncoderBuilder;

use super::{copy_to, Compressor, Decompressor, Result};


pub struct Lz4Compressor
{

}

impl Lz4Compressor {
    pub fn new() -> Lz4Compressor
    {
        Lz4Compressor{}
    }
}

impl Compressor for Lz4Compressor 
{
    fn compress<R: Read, W: Write>(&mut self, input: &mut R, output: &mut W) -> Result<usize>
    {
        let mut encoder = EncoderBuilder::new()
                    .level(4)
                    .build(output)?;
        let res = copy_to(input, &mut encoder)?;
        _ = encoder.finish();
        Ok(res)
    }
}


impl Decompressor for Lz4Compressor
{
    fn decompress<R: Read, W: Write>(&mut self, input: &mut R, output: &mut W) -> Result<usize>
    {
        let mut decoder = lz4::Decoder::new(input)?;
        copy_to(&mut decoder, output)
    }
}