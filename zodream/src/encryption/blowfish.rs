use cipher::{generic_array::GenericArray, BlockEncrypt, BlockDecrypt, KeyInit};

use super::{Encryptor, Decryptor};


pub struct Blowfish 
{
    instance: ::blowfish::Blowfish,
}

impl Blowfish {
    pub fn new(key: &[u8]) -> Blowfish
    {
        Blowfish{instance: ::blowfish::Blowfish::new(GenericArray::from_slice(key)) }
    }
    
}

impl Encryptor for Blowfish 
{
    fn block_size(&self) -> usize
    {
        16
    }
    fn encrypt(&mut self, input: &[u8], output: &mut [u8]) -> usize
    {
        let data = GenericArray::from_slice(input);
        let mut out = GenericArray::from_mut_slice(output);
        self.instance.encrypt_block_b2b(data, &mut out);
        input.len()
    }
}

impl Decryptor for Blowfish 
{
    fn block_size(&self) -> usize
    {
        16
    }
    fn decrypt(&mut self, input: &[u8], output: &mut [u8]) -> usize
    {
        let data = GenericArray::from_slice(input);
        let mut out = GenericArray::from_mut_slice(output);
        self.instance.decrypt_block_b2b(data, &mut out);
        input.len()
    }
}