use inout::InOutBuf;
use rc4::{Key, Rc4};
use rc4::{consts::*, KeyInit};
use cipher::StreamCipher;

use super::{Encryptor, Decryptor, Result};


pub struct Arc4 
{
    instance: Rc4<U6>,
}

impl Arc4 {
    pub fn new(key: &[u8]) -> Arc4
    {
        let rc4 = Rc4::<_>::new(Key::<U6>::from_slice(key));
        Arc4{instance: rc4}
    }
    
}

impl Encryptor for Arc4
{
    fn block_size(&self) -> usize
    {
        0
    }
    fn encrypt(&mut self, input: &[u8], output: &mut [u8]) -> Result<usize>
    {

        self.instance.try_apply_keystream_inout(InOutBuf::new(input, output)?)?;
        Ok(output.len())
    }
}

impl Decryptor for Arc4
{
    fn block_size(&self) -> usize
    {
        0
    }
    fn decrypt(&mut self, input: &[u8], output: &mut [u8]) -> Result<usize>
    {
        self.instance.try_apply_keystream_inout(InOutBuf::new(input, output)?)?;
        Ok(output.len())
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_arc4() {
        let key = b"hellewordaaa";
        let input = b"aaafkdnkjank";
        let mut encrypted = vec![0u8; input.len()];
        let mut decrypted = vec![0u8; input.len()];
        let mut instance1 = Arc4::new(key);
        let mut instance2 = Arc4::new(key);
        _ = instance1.encrypt(input, encrypted.as_mut_slice());
        _ = instance2.decrypt(encrypted.as_slice(), decrypted.as_mut_slice());
        assert_eq!(input, decrypted.as_slice());
    }

}