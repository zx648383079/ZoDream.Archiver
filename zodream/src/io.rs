use std::io::{Read, Write};
use super::error::Result;

#[doc(hidden)]
const CONTINUATION_BIT: u8 = 1 << 7;
#[doc(hidden)]
const SIGN_BIT: u8 = 1 << 6;

pub trait ByteReadExt : Read {
    #[inline]
    fn read_bytes(&mut self, count: u64) -> Result<Vec<u8>>
    {
        let mut buf = Vec::with_capacity(count as usize);
        self.read_exact(&mut buf)?;
        Ok(buf)
    }
    #[inline]
    fn read_u8(&mut self) -> Result<u8>
    {
        let mut buf = [0; 1];
        self.read_exact(&mut buf)?;
        Ok(buf[0])
    }
    #[inline]
    fn read_u16_le(&mut self) -> Result<u16> 
    {
        let mut buf = [0; 2];
        self.read_exact(&mut buf)?;
        Ok(u16::from_le_bytes(buf))
    }
    #[inline]
    fn read_u32_le(&mut self) -> Result<u32>
    {
        let mut buf = [0; 4];
        self.read_exact(&mut buf)?;
        Ok(u32::from_le_bytes(buf))
    }
    #[inline]
    fn read_u64_le(&mut self) -> Result<u64> 
    {
        let mut buf = [0; 8];
        self.read_exact(&mut buf)?;
        Ok(u64::from_le_bytes(buf))
    }
    #[inline]
    fn read_u128_le(&mut self) -> Result<u128>
    {
        let mut buf = [0; 16];
        self.read_exact(&mut buf)?;
        Ok(u128::from_le_bytes(buf))
    }
    #[inline]
    fn read_i16_le(&mut self) -> Result<i16> 
    {
        let mut buf = [0; 2];
        self.read_exact(&mut buf)?;
        Ok(i16::from_le_bytes(buf))
    }
    #[inline]
    fn read_i32_le(&mut self) -> Result<i32> 
    {
        let mut buf = [0; 4];
        self.read_exact(&mut buf)?;
        Ok(i32::from_le_bytes(buf))
    }
    #[inline]
    fn read_i64_le(&mut self) -> Result<i64> 
    {
        let mut buf = [0; 8];
        self.read_exact(&mut buf)?;
        Ok(i64::from_le_bytes(buf))
    }
    #[inline]
    fn read_i128_le(&mut self) -> Result<i128> 
    {
        let mut buf = [0; 16];
        self.read_exact(&mut buf)?;
        Ok(i128::from_le_bytes(buf))
    }

    #[inline]
    fn read_u16_be(&mut self) -> Result<u16> 
    {
        let mut buf = [0; 2];
        self.read_exact(&mut buf)?;
        Ok(u16::from_be_bytes(buf))
    }
    #[inline]
    fn read_u32_be(&mut self) -> Result<u32> 
    {
        let mut buf = [0; 4];
        self.read_exact(&mut buf)?;
        Ok(u32::from_be_bytes(buf))
    }
    #[inline]
    fn read_u64_be(&mut self) -> Result<u64> 
    {
        let mut buf = [0; 8];
        self.read_exact(&mut buf)?;
        Ok(u64::from_be_bytes(buf))
    }
    #[inline]
    fn read_u128_be(&mut self) -> Result<u128> 
    {
        let mut buf = [0; 16];
        self.read_exact(&mut buf)?;
        Ok(u128::from_be_bytes(buf))
    }
    #[inline]
    fn read_i16_be(&mut self) -> Result<i16> 
    {
        let mut buf = [0; 2];
        self.read_exact(&mut buf)?;
        Ok(i16::from_be_bytes(buf))
    }
    #[inline]
    fn read_i32_be(&mut self) -> Result<i32> 
    {
        let mut buf = [0; 4];
        self.read_exact(&mut buf)?;
        Ok(i32::from_be_bytes(buf))
    }
    #[inline]
    fn read_i64_be(&mut self) -> Result<i64> 
    {
        let mut buf = [0; 8];
        self.read_exact(&mut buf)?;
        Ok(i64::from_be_bytes(buf))
    }
    #[inline]
    fn read_i128_be(&mut self) -> Result<i128> 
    {
        let mut buf = [0; 16];
        self.read_exact(&mut buf)?;
        Ok(i128::from_be_bytes(buf))
    }

    #[inline]
    fn read_f32_be(&mut self) -> Result<f32> 
    {
        let mut buf = [0; 4];
        self.read_exact(&mut buf)?;
        Ok(f32::from_be_bytes(buf))
    }
    #[inline]
    fn read_f64_be(&mut self) -> Result<f64> 
    {
        let mut buf = [0; 8];
        self.read_exact(&mut buf)?;
        Ok(f64::from_be_bytes(buf))
    }

    #[inline]
    fn read_f32_le(&mut self) -> Result<f32> 
    {
        let mut buf = [0; 4];
        self.read_exact(&mut buf)?;
        Ok(f32::from_le_bytes(buf))
    }
    #[inline]
    fn read_f64_le(&mut self) -> Result<f64> 
    {
        let mut buf = [0; 8];
        self.read_exact(&mut buf)?;
        Ok(f64::from_le_bytes(buf))
    }

    #[inline]
    fn read_boolean(&mut self) -> Result<bool> 
    {
        Ok(self.read_u8()? == 1)
    }
    #[inline]
    fn read_leb128_u16(&mut self) -> Result<u16> 
    {
        Ok(self.read_leb128_unsigned(2)? as u16)
    }
    #[inline]
    fn read_leb128_u32(&mut self) -> Result<u32> 
    {
        Ok(self.read_leb128_unsigned(4)? as u32)
    }
    #[inline]
    fn read_leb128_u64(&mut self) -> Result<u64> 
    {
        self.read_leb128_unsigned(8)
    }
    #[inline]
    fn read_leb128_u128(&mut self) -> Result<u128> 
    {
        let mut result = 0u128;
        let mut shift = 0;
        loop {
            let code = self.read_u8()?;
            result |= ((code & 0x7f) as u128)  << shift;
            if code & CONTINUATION_BIT == 0 {
                break;
            }
            shift += 7;
        }
        Ok(result)
    }
    #[inline]
    fn read_leb128_i16(&mut self) -> Result<i16> 
    {
        Ok(self.read_leb128_signed(2)? as i16)
    }
    #[inline]
    fn read_leb128_i32(&mut self) -> Result<i32> 
    {
        Ok(self.read_leb128_signed(4)? as i32)
    }
    #[inline]
    fn read_leb128_i64(&mut self) -> Result<i64> 
    {
        self.read_leb128_signed(8)
    }
    #[inline]
    fn read_leb128_i128(&mut self) -> Result<i128> 
    {
        let mut result = 0i128;
        let mut shift = 0;
        let mut i = 0;
        let mut is_negative = false;
        loop {
            let code = self.read_u8()?;
            if i == 0 {
                is_negative = code & SIGN_BIT != 0;
            }
            i += 1;
            result |= ((code & 0x7f) as i128)  << shift;
            if code & CONTINUATION_BIT == 0 {
                break;
            }
            shift += 7;
        }
        if is_negative {
            result |= -(1 << shift);
        }
        Ok(result)
    }
    #[inline]
    fn read_leb128_unsigned(&mut self, max_bits: usize) -> Result<u64> 
    {
        let mut result = 0u64;
        let mut shift = 0;
        let mut i = 0;
        loop {
            let code = self.read_u8()?;
            i += 1;
            if max_bits > 0 && i >= max_bits {
                result |= (code as u64) << shift;
                break;
            }
            result |= ((code & 0x7f) as u64)  << shift;
            if code & CONTINUATION_BIT == 0 {
                break;
            }
            shift += 7;
        }
        Ok(result)
    }
    #[inline]
    fn read_leb128_signed(&mut self, max_bits: usize) -> Result<i64> 
    {
        let mut result = 0i64;
        let mut shift = 0;
        let mut i = 0;
        let mut is_negative = false;
        loop {
            let code = self.read_u8()?;
            if i == 0 {
                is_negative = code & SIGN_BIT != 0;
            }
            i += 1;
            if max_bits > 0 && i >= max_bits {
                result |= (code as i64) << shift;
                break;
            }
            result |= ((code & 0x7f) as i64)  << shift;
            if code & CONTINUATION_BIT == 0 {
                break;
            }
            shift += 7;
        }
        if is_negative {
            result |= -(1 << shift);
        }
        Ok(result)
    }
}

impl<R: Read> ByteReadExt for R {
    
}

// impl<T> ByteReadExt for Cursor<T> where T : AsRef<[u8]> {
    
// }


pub trait ByteWriteExt : Write {

    #[inline]
    fn write_u8(&mut self, val: u8) -> Result<usize>
    {
        let buf = [val];
        Ok(self.write(&buf)?)
    }
    #[inline]
    fn write_u16_le(&mut self, val: u16) -> Result<usize>
    {
        Ok(self.write(&val.to_le_bytes())?)
    }
    #[inline]
    fn write_u32_le(&mut self, val: u32) -> Result<usize>
    {
        Ok(self.write(&val.to_le_bytes())?)
    }
    #[inline]
    fn write_u64_le(&mut self, val: u64) -> Result<usize>
    {
        Ok(self.write(&val.to_le_bytes())?)
    }
    #[inline]
    fn write_u128_le(&mut self, val: u128) -> Result<usize>
    {
        Ok(self.write(&val.to_le_bytes())?)
    }
    #[inline]
    fn write_i16_le(&mut self, val: i16) -> Result<usize>
    {
        Ok(self.write(&val.to_le_bytes())?)
    }
    #[inline]
    fn write_i32_le(&mut self, val: i32) -> Result<usize>
    {
        Ok(self.write(&val.to_le_bytes())?)
    }
    #[inline]
    fn write_i64_le(&mut self, val: i64) -> Result<usize>
    {
        Ok(self.write(&val.to_le_bytes())?)
    }
    #[inline]
    fn write_i128_le(&mut self, val: i128) -> Result<usize>
    {
        Ok(self.write(&val.to_le_bytes())?)
    }
    #[inline]
    fn write_u16_be(&mut self, val: u16) -> Result<usize>
    {
        Ok(self.write(&val.to_be_bytes())?)
    }
    #[inline]
    fn write_u32_be(&mut self, val: u32) -> Result<usize>
    {
        Ok(self.write(&val.to_be_bytes())?)
    }
    #[inline]
    fn write_u64_be(&mut self, val: u64) -> Result<usize>
    {
        Ok(self.write(&val.to_be_bytes())?)
    }
    #[inline]
    fn write_u128_be(&mut self, val: u128) -> Result<usize>
    {
        Ok(self.write(&val.to_be_bytes())?)
    }
    #[inline]
    fn write_i16_be(&mut self, val: i16) -> Result<usize>
    {
        Ok(self.write(&val.to_be_bytes())?)
    }
    #[inline]
    fn write_i32_be(&mut self, val: i32) -> Result<usize>
    {
        Ok(self.write(&val.to_be_bytes())?)
    }
    #[inline]
    fn write_i64_be(&mut self, val: i64) -> Result<usize>
    {
        Ok(self.write(&val.to_be_bytes())?)
    }
    #[inline]
    fn write_i128_be(&mut self, val: i128) -> Result<usize>
    {
        Ok(self.write(&val.to_be_bytes())?)
    }
    #[inline]
    fn write_boolean(&mut self, val: bool) -> Result<usize>
    {
        self.write_u8(if val {1} else {0})
    }
    #[inline]
    fn write_leb128_unsigned(&mut self, val: u64) -> Result<usize>
    {
        let mut val = val;
        let mut bytes_written = 0;
        loop {
            let mut byte = ((val & (std::u8::MAX as u64)) as u8) & !CONTINUATION_BIT;
            val >>= 7;
            if val != 0 {
                // More bytes to come, so set the continuation bit.
                byte |= CONTINUATION_BIT;
            }

            let buf = [byte];
            self.write_all(&buf)?;
            bytes_written += 1;

            if val == 0 {
                return Ok(bytes_written);
            }
        }
    }
    #[inline]
    fn write_leb128_signed(&mut self, val: i64) -> Result<usize>
    {
        let mut val = val;
        let mut bytes_written = 0;
        loop {
            let mut byte = val as u8;
            // Keep the sign bit for testing
            val >>= 6;
            let done = val == 0 || val == -1;
            if done {
                byte &= !CONTINUATION_BIT;
            } else {
                val >>= 1;
                byte |= CONTINUATION_BIT;
            }

            let buf = [byte];
            self.write_all(&buf)?;
            bytes_written += 1;

            if done {
                return Ok(bytes_written);
            }
        }
    }
}


impl<W: Write> ByteWriteExt for W {
    
}