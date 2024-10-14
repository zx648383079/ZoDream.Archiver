use cipher::{generic_array::GenericArray, BlockDecryptMut, BlockEncryptMut, KeyIvInit};
use rc4::KeyInit;

use super::{Encryptor, Decryptor};

type BlowfishCbcEnc = cbc::Encryptor<::blowfish::Blowfish>;
type BlowfishCbcDec = cbc::Decryptor<::blowfish::Blowfish>;

pub struct BlowfishCbcEncryptor
{
    instance: BlowfishCbcEnc,
}

impl Encryptor for BlowfishCbcEncryptor
{
    fn block_size(&self) -> usize
    {
        16
    }
    fn encrypt(&mut self, input: &[u8], output: &mut [u8]) -> usize
    {
        let data = GenericArray::from_slice(input);
        let mut out = GenericArray::from_mut_slice(output);
        self.instance.encrypt_block_b2b_mut(data, &mut out);
        input.len()
    }
}

pub struct BlowfishCbcDecryptor
{
    instance: BlowfishCbcDec,
}


impl Decryptor for BlowfishCbcDecryptor
{
    fn block_size(&self) -> usize
    {
        16
    }
    fn decrypt(&mut self, input: &[u8], output: &mut [u8]) -> usize
    {
        let data = GenericArray::from_slice(input);
        let mut out = GenericArray::from_mut_slice(output);
        self.instance.decrypt_block_b2b_mut(data, &mut out);
        input.len()
    }
}



pub struct Blowfish 
{
    instance: ::blowfish::Blowfish,
}

impl Blowfish {
    pub fn new(key: &[u8]) -> Blowfish
    {
        Blowfish{instance: ::blowfish::Blowfish::new_from_slice(key).unwrap() }
    }
    pub fn new_cbc_enc(key: &[u8]) -> BlowfishCbcEncryptor
    {
        let iv = [0u8; 16];
        BlowfishCbcEncryptor{instance: BlowfishCbcEnc::new_from_slices(key, &iv).unwrap() }
    }
    pub fn new_cbc_dec(key: &[u8]) -> BlowfishCbcDecryptor
    {
        let iv = [0u8; 16];
        BlowfishCbcDecryptor{instance: BlowfishCbcDec::new_from_slices(key, &iv).unwrap() }
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
        self.instance.encrypt_block_b2b_mut(data, &mut out);
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
        self.instance.decrypt_block_b2b_mut(data, &mut out);
        input.len()
    }
}