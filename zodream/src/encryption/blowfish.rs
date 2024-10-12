use cipher::{generic_array::GenericArray, BlockEncrypt, BlockDecrypt, KeyInit};

use super::encryptor::{Encryptor, Decryptor};


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
    fn block_size(&self) -> u32 
    {
        16
    }
    fn encrypt(&mut self, input: &[u8], output: &mut [u8]) -> u64
    {
        let data = GenericArray::from_slice(input);
        let mut out = GenericArray::from_mut_slice(output);
        self.instance.encrypt_block_b2b(data, &mut out);
        0
    }
}

impl Decryptor for Blowfish 
{
    fn block_size(&self) -> u32 
    {
        16
    }
    fn decrypt(&mut self, input: &[u8], output: &mut [u8]) -> u64
    {
        let data = GenericArray::from_slice(input);
        let mut out = GenericArray::from_mut_slice(output);
        self.instance.decrypt_block_b2b(data, &mut out);
        0
    }
}