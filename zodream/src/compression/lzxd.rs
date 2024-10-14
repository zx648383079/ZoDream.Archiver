use std::io::{Read, Write};

use lzxd::{Lzxd, WindowSize};

use super::Decompressor;


pub struct LzxdCompressor
{

}

impl LzxdCompressor {
    pub fn new() -> LzxdCompressor
    {
        LzxdCompressor{}
    }
}


impl Decompressor for LzxdCompressor
{
    fn decompress<R: Read, W: Write>(&mut self, input: &mut R, output: &mut W) -> u64
    {
        let mut buf = [0u8; 8];

        // Read file header.
        input.read_exact(&mut buf[..4]).unwrap();
        let ws = u32::from_le_bytes(buf[..4].try_into().unwrap());
        input.read_exact(&mut buf[..4]).unwrap(); // Discard.

        let ws = match ws {
            0x0000_8000 => WindowSize::KB32,
            0x0001_0000 => WindowSize::KB64,
            0x0002_0000 => WindowSize::KB128,
            0x0004_0000 => WindowSize::KB256,
            0x0008_0000 => WindowSize::KB512,
            0x0010_0000 => WindowSize::MB1,
            0x0020_0000 => WindowSize::MB2,
            0x0040_0000 => WindowSize::MB4,
            0x0080_0000 => WindowSize::MB8,
            0x0100_0000 => WindowSize::MB16,
            0x0200_0000 => WindowSize::MB32,
            _ => panic!("invalid window size"),
        };

        let mut lzxd = Lzxd::new(ws);
        let mut chunk = Vec::new();
        let mut length = 0u64;

        loop {
            match input.read(&mut buf[..8]) {
                // Check for the end of the stream.
                Ok(n) if n < 8 => break,
                Err(_) => break,

                Ok(_) => {}
            }

            let chunk_len = usize::from_le_bytes(buf.try_into().unwrap());
            input.read_exact(&mut buf[..8]).unwrap();
            let output_len = usize::from_le_bytes(buf.try_into().unwrap());

            chunk.resize(chunk_len, 0);

            input.read_exact(&mut chunk).unwrap();
            
            let res = lzxd.decompress_next(&mut chunk, output_len).unwrap();
            output.write(res).unwrap();
            length += res.len() as u64;
        }
        length
    }
}