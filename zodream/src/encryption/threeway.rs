use super::{Encryptor, Decryptor, Result};

const STRT_E: u32 =  0x0b0b;
const STRT_D: u32 =  0xb1b1;

pub struct ThreeWay 
{
    key: [u32;3]
}

impl ThreeWay {
    pub fn new(key: &[u8]) -> ThreeWay
    {
        let mut buffer = [0u32; 3];
        for i in 0..buffer.len() {
            let index = i * 4;
            buffer[i] = (key[index + 0] as u32) | ((key[index + 1] as u32) << 8) | ((key[index + 2] as u32) << 16) | ((key[index + 3] as u32) << 24);
        }
        ThreeWay{key: buffer.to_owned()}
    }
    
    fn mu(data: &mut [u32]) 
    {
        let mut buffer = [0u32; 3];
        for _ in 0..32 {
            for j in 0..buffer.len() {
                buffer[j] <<= 1;
            }
            for j in 0..buffer.len() {
                if (data[j] & 1) > 0 {
                    buffer[buffer.len() - j - 1] |= 1;
                }
                data[j] >>= 1;
            }
        }
        data.copy_from_slice(buffer.as_slice());
    }

    fn way3_gamma(data: &mut [u32]) 
    {
        let mut buffer = [0u32; 3];
        for i in 0..buffer.len() {
            buffer[i] = data[i] ^ (data[(i + 1) % 3] | (! data[(i + 2) % 3]));
        }
        data.copy_from_slice(buffer.as_slice());
    }

    fn theta(data: &mut [u32]) 
    {
        let mut buffer = [0u32; 3];
        for i in 0..buffer.len() {
            let a0 = data[i];
            let a1 = data[(i + 1) % 3];
            let a2 = data[(i + 2) % 3];
            buffer[i] = a0 ^ (a0 >> 16) ^ (a1<<16) ^     (a1>>16) ^ (a2<<16) ^ (a1>>24) ^ (a2<<8)  ^     (a2>>8)  ^ (a0<<24) ^ (a2>>16) ^ (a0<<16) ^     (a2>>24) ^ (a0<<8);
        }
        data.copy_from_slice(buffer.as_slice());
    }

    fn pi_1(data: &mut [u32])
    {
        data[0] = (data[0]>>10) ^ (data[0]<<22);  
        data[2] = (data[2]<<1)  ^ (data[2]>>31);
    }

    fn pi_2(data: &mut [u32])
    {
        data[0] = (data[0]<<1)  ^ (data[0]>>31);
        data[2] = (data[2]>>10) ^ (data[2]<<22);
    }

    fn rho(data: &mut [u32])
    {
        Self::theta(data); 
        Self::pi_1(data); 
        Self::way3_gamma(data); 
        Self::pi_2(data);
    }

    fn rndcon_gen(strt: u32, data: &mut [u32])
    {
        let mut j = strt;
        for i in 0..12 {
            data[i] = j;
            j <<= 1;
            if j & 0x10000 != 0 {
                j ^= 0x11011;
            }
        }
    }
}

impl Encryptor for ThreeWay 
{
    fn block_size(&self) -> usize
    {
        12
    }
    fn encrypt(&mut self, input: &[u8], output: &mut [u8]) -> Result<usize>
    {
        let mut rcon = [0u32; 12];
        Self::rndcon_gen(STRT_E, & mut rcon);
        let mut data = [0u32; 3];
        for i in 0..data.len() {
            let j = i * 4;
            data[i] = u32::from_le_bytes(input[j..(j+4)].try_into().unwrap());
        }
        for i in 0..12 {
            data[0] ^= self.key[0] as u32 ^ (rcon[i]<<16) ; 
            data[1] ^= self.key[1] ; 
            data[2] ^= self.key[2] ^ rcon[i];
            if i < 11 {
                Self::rho(&mut data);
            } else {
                Self::theta(&mut data) ;
            }
        }
        for i in 0..data.len() {
            let j = i * 4;
            output[j..(j+4)].copy_from_slice(&data[i].to_le_bytes());
        }
        Ok(data.len())
    }
}

impl Decryptor for ThreeWay 
{
    fn block_size(&self) -> usize
    {
        12
    }
    fn decrypt(&mut self, input: &[u8], output: &mut [u8]) -> Result<usize>
    {
        let mut ki = self.key.clone();
        Self::theta(&mut ki);
        Self::mu(&mut ki);
        println!("{:?}", ki);

        let mut rcon = [0u32; 12];
        Self::rndcon_gen(STRT_D, & mut rcon);
        let mut data = [0u32; 3];
        for i in 0..data.len() {
            let j = i * 4;
            data[i] = u32::from_le_bytes(input[j..(j+4)].try_into().unwrap());
        }
        Self::mu(&mut data);
        for i in 0..12 {
            data[0] ^= ki[0] as u32 ^ (rcon[i]<<16) ; 
            data[1] ^= ki[1] ; 
            data[2] ^= ki[2] ^ rcon[i];
            if i < 11 {
                Self::rho(&mut data);
            } else {
                Self::theta(&mut data) ;
            }
        }
        Self::mu(&mut data);
        for i in 0..data.len() {
            let j = i * 4;
            output[j..(j+4)].copy_from_slice(&data[i].to_le_bytes());
        }
        Ok(data.len())
    }
}


#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_3way_key() {
        let key = b"hellewordaaa";
        let instance = ThreeWay::new(key);
        assert_eq!(instance.key, [1819043176, 1919907685, 1633771876]);
    }

}