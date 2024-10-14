use std::io::{Read, Write};

use lz4::EncoderBuilder;

use super::{copy_to, Compressor, Decompressor};


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
    fn compress<R: Read, W: Write>(&mut self, input: &mut R, output: &mut W) -> u64
    {
        let mut encoder = EncoderBuilder::new()
                    .level(4)
                    .build(output).unwrap();
        let res = copy_to(input, &mut encoder);
        _ = encoder.finish();
        res
    }
}


impl Decompressor for Lz4Compressor
{
    fn decompress<R: Read, W: Write>(&mut self, input: &mut R, output: &mut W) -> u64
    {
        let mut decoder = lz4::Decoder::new(input).unwrap();
        copy_to(&mut decoder, output)
    }
}