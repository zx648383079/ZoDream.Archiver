use std::io::{Read, Write};

pub trait Encryptor {
    fn block_size(&self) -> u32;
    fn encrypt(&mut self, input: &[u8], output: &mut [u8]) -> u64;
}

fn encrypt_stream<T, R, W>(mut target: T, mut input: R, mut output: W) -> u64 
    where R : Read, W : Write, T : Encryptor
{
    let mut len: u64 = 0;
    let size = target.block_size();
    let mut in_buf = vec![0u8; size as usize];
    let mut out_buf = vec![0u8; size as usize];
    let mut l = size;
    while l == size {
        l = match input.read(in_buf.as_mut_slice()) {
            Ok(i) => i as u32,
            Err(_) => 0,
        };
        if l == 0 {
            break;
        }
        let c = target.encrypt(in_buf.as_slice(), out_buf.as_mut_slice());
        output.write(out_buf.as_slice());
        len += c;
    }
    len
}

pub trait Decryptor {
    fn block_size(&self) -> u32;
    fn decrypt(&mut self, input: &[u8], output: &mut [u8]) -> u64;

}

fn decrypt_stream<T, R, W>(mut target: T, mut input: R, mut output: W) -> u64 
    where R : Read, W : Write, T : Decryptor
{
    let mut len: u64 = 0;
    let size = target.block_size();
    let mut in_buf = vec![0u8; size as usize];
    let mut out_buf = vec![0u8; size as usize];
    let mut l = size;
    while l == size {
        l = match input.read(in_buf.as_mut_slice()) {
            Ok(i) => i as u32,
            Err(_) => 0,
        };
        if l == 0 {
            break;
        }
        let c = target.decrypt(in_buf.as_slice(), out_buf.as_mut_slice());
        output.write(out_buf.as_slice());
        len += c;
    }
    len
}

// let test = "0000000000000000";
// let test = test.as_bytes();

// println!("test: {:?}", test);

// let key = GenericArray::from([48u8; 16]);
// let mut block = GenericArray::from([48u8; 16]);
// // Initialize cipher
// let cipher = Aes128::new(&key);
// // Encrypt block in-place
// println!("明文: {:?}", block);
// cipher.encrypt_block(&mut block);
// println!("密文: {:?}", base64::encode(block));

// cipher.decrypt_block(&mut block);
// println!("明文: {:?}", std::str::from_utf8(&block));