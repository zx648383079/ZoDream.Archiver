pub mod blowfish;
pub mod own;
pub mod threeway;
pub mod arc4;

use std::io::{Read, Write};

const DEFAULT_BLOCK_SIZE: usize = 1024;

pub trait Encryptor {
    fn block_size(&self) -> usize;
    fn encrypt(&mut self, input: &[u8], output: &mut [u8]) -> usize;
}

pub fn encrypt_stream<T, R, W>(target: & mut T, input: &mut R, output: &mut  W) -> u64 
    where R : Read, W : Write, T : Encryptor + ?Sized
{
    let mut len: u64 = 0;
    let mut size = target.block_size();
    if size == 0 {
        size = DEFAULT_BLOCK_SIZE;
    }
    let mut in_buf = vec![0u8; size];
    let mut out_buf = vec![0u8; size];
    let mut l = size;
    while l == size {
        l = match input.read(in_buf.as_mut_slice()) {
            Ok(i) => i,
            Err(_) => 0,
        };
        if l == 0 {
            break;
        }
        let c = target.encrypt(&in_buf[..l], out_buf.as_mut_slice());
        _ = output.write(&out_buf[..c]);
        len += c as u64;
    }
    len
}

pub trait Decryptor {
    fn block_size(&self) -> usize;
    fn decrypt(&mut self, input: &[u8], output: &mut [u8]) -> usize;
}

pub fn decrypt_stream<T, R, W>(target: & mut T, input: &mut R, output: &mut W) -> u64 
    where R : Read, W : Write, T : Decryptor
{
    let mut len: u64 = 0;
    let mut size = target.block_size();
    if size == 0 {
        size = DEFAULT_BLOCK_SIZE;
    }
    let mut in_buf = vec![0u8; size];
    let mut out_buf = vec![0u8; size];
    let mut l = size;
    while l == size {
        l = match input.read(in_buf.as_mut_slice()) {
            Ok(i) => i,
            Err(_) => 0,
        };
        if l == 0 {
            break;
        }
        let c = target.decrypt(&in_buf[..l], out_buf.as_mut_slice());
        _ = output.write(&out_buf[..c]);
        len += c as u64;
    }
    len
}