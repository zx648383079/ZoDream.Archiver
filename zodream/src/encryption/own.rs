use super::{Encryptor, Decryptor};


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
        1024
    }
    fn encrypt(&mut self, input: &[u8], output: &mut [u8]) -> usize
    {
        let size = input.len();
        for i in 0..size {
            if input[i] > 128 {
                output[i] = input[i] - 9;
            } else {
                output[i] = input[i] + 9;
            }
        }
        size
    }
}

impl Decryptor for OwnEncryptor 
{
    fn block_size(&self) -> usize
    {
        1024
    }
    fn decrypt(&mut self, input: &[u8], output: &mut [u8]) -> usize
    {
        let size = input.len();
        for i in 0..size {
            if input[i] > 128 {
                output[i] = input[i] - 9;
            } else {
                output[i] = input[i] + 9;
            }
        }
        size
    }
}

