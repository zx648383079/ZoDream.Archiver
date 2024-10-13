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
                output[i] = input[i] + 9;
            } else {
                output[i] = input[i] - 9;
            }
        }
        size
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_own() {
        let key = b"helle";
        let input = b"aaafkdnkjank";
        let mut encrypted = vec![0u8; input.len()];
        let mut decrypted = vec![0u8; input.len()];
        let mut instance = OwnEncryptor::new(key);
        _ = instance.encrypt(input, encrypted.as_mut_slice());
        _ = instance.decrypt(encrypted.as_slice(), decrypted.as_mut_slice());
        assert_eq!(input, decrypted.as_slice());
    }

}
