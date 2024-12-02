use std::io::{Read, Write};
use super::error::Result;

pub mod lzxd;
pub mod lz4;
pub mod lua;

const DEFAULT_BLOCK_SIZE: usize = 1024;

/// 压缩
pub trait Compressor {
    fn compress<R: Read, W: Write>(&mut self, input: &mut R, output: &mut W) -> Result<usize>;
}

/// 解压缩
pub trait Decompressor {
    fn decompress<R: Read, W: Write>(&mut self, input: &mut R, output: &mut W) -> Result<usize>;
}

/// 复制数据
pub fn copy_to<R, W>(input: &mut R, output: &mut  W) -> Result<usize>
    where R : Read, W : Write
{
    let mut len: u64 = 0;
    let size = DEFAULT_BLOCK_SIZE;
    let mut in_buf = vec![0u8; size];
    let mut l = size;
    while l == size {
        l = match input.read(in_buf.as_mut_slice()) {
            Ok(i) => i,
            Err(_) => 0,
        };
        if l == 0 {
            break;
        }
        _ = output.write(&in_buf[..l]);
        len += l as u64;
    }
    Ok(len as usize)
}