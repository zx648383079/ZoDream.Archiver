use std::io::{Read, Write};

pub mod lzxd;

pub trait Compressor {
    fn compress<R: Read, W: Write>(&mut self, input: &mut R, output: &mut W) -> u64;
}

pub trait Decompressor {
    fn decompress<R: Read, W: Write>(&mut self, input: &mut R, output: &mut W) -> u64;
}