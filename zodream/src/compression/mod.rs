pub trait Compressor {
    fn new(input: Read) -> Compressor;
    fn compress(&mut self, output: Write) -> u64;
}

pub trait Decompressor {
    fn new(input: Read) -> Compressor;
    fn decompress(&mut self, output: Write) -> u64;
}