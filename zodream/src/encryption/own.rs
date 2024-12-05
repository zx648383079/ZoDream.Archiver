use super::{Encryptor, Decryptor, Result};


pub struct OwnEncryptor 
{

}

impl OwnEncryptor {
    pub fn new(_key: &[u8]) -> OwnEncryptor
    {
        OwnEncryptor{}
    }
    
}

impl Encryptor for OwnEncryptor 
{
    fn block_size(&self) -> usize
    {
        0
    }
    fn encrypt(&mut self, input: &[u8], output: &mut [u8]) -> Result<usize>
    {
        let size = input.len();
        for i in 0..size {
            if input[i] > 128 {
                output[i] = input[i] - 9;
            } else {
                output[i] = input[i] + 9;
            }
        }
        Ok(size)
    }
}

impl Decryptor for OwnEncryptor 
{
    fn block_size(&self) -> usize
    {
        0
    }
    fn decrypt(&mut self, input: &[u8], output: &mut [u8]) -> Result<usize>
    {
        let size = input.len();
        for i in 0..size {
            if input[i] > 128 {
                output[i] = input[i] + 9;
            } else {
                output[i] = input[i] - 9;
            }
        }
        Ok(size)
    }
}
