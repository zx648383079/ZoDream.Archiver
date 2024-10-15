use cipher::{generic_array::GenericArray, BlockDecryptMut, BlockEncryptMut, KeyIvInit};
use rc4::KeyInit;

use super::{Encryptor, Decryptor, Result};

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
        8
    }
    fn encrypt(&mut self, input: &[u8], output: &mut [u8]) -> Result<usize>
    {
        let data = GenericArray::from_slice(input);
        let mut out = GenericArray::from_mut_slice(output);
        self.instance.encrypt_block_b2b_mut(data, &mut out);
        Ok(input.len())
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
        8
    }
    fn decrypt(&mut self, input: &[u8], output: &mut [u8]) -> Result<usize>
    {
        let data = GenericArray::from_slice(input);
        let mut out = GenericArray::from_mut_slice(output);
        self.instance.decrypt_block_b2b_mut(data, &mut out);
        Ok(input.len())
    }
}



pub struct Blowfish 
{
    instance: ::blowfish::Blowfish,
}

impl Blowfish {
    pub fn new(key: &[u8]) -> Result<Blowfish>
    {
        let res = ::blowfish::Blowfish::new_from_slice(key)?;
        Ok(Blowfish{instance: res })
    }
    pub fn new_cbc_enc(key: &[u8]) -> Result<BlowfishCbcEncryptor>
    {
        let iv = [0u8; 8];
        let res = BlowfishCbcEnc::new_from_slices(key, &iv)?;
        Ok(BlowfishCbcEncryptor{instance: res })
    }
    pub fn new_cbc_dec(key: &[u8]) -> Result<BlowfishCbcDecryptor>
    {
        let iv = [0u8; 8];
        let res = BlowfishCbcDec::new_from_slices(key, &iv)?;
        Ok(BlowfishCbcDecryptor{instance: res })
    }
}

impl Encryptor for Blowfish
{
    fn block_size(&self) -> usize
    {
        8
    }
    fn encrypt(&mut self, input: &[u8], output: &mut [u8]) -> Result<usize>
    {
        let data = GenericArray::from_slice(input);
        let mut out = GenericArray::from_mut_slice(output);
        self.instance.encrypt_block_b2b_mut(data, &mut out);
        Ok(input.len())
    }
}

impl Decryptor for Blowfish 
{
    fn block_size(&self) -> usize
    {
        8
    }
    fn decrypt(&mut self, input: &[u8], output: &mut [u8]) -> Result<usize>
    {
        let data = GenericArray::from_slice(input);
        let mut out = GenericArray::from_mut_slice(output);
        self.instance.decrypt_block_b2b_mut(data, &mut out);
        Ok(input.len())
    }
}