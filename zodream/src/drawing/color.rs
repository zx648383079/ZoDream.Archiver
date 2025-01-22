#![allow(dead_code)]

use std::io::{Read, Write};

use super::super::error::Result;

pub trait ColorReadExt : Read {
    fn read_bgra(&mut self) -> Result<u32>
    {
        let mut buf = [0; 4];
        self.read_exact(&mut buf)?;
        buf.swap(0, 2);
        Ok(u32::from_le_bytes(buf))
    }
    fn read_rgba(&mut self) -> Result<u32>
    {
        let mut buf = [0; 4];
        self.read_exact(&mut buf)?;
        Ok(u32::from_le_bytes(buf))
    }
}

impl<R: Read> ColorReadExt for R {
    
}

pub trait ColorWriteExt : Write {
    fn write_bgra(&mut self, val: u32) -> Result<usize>
    {
        let mut buf = val.to_le_bytes();
        buf.swap(0, 2);
        Ok(self.write(&buf)?)
    }
    fn write_rgba(&mut self, val: u32) -> Result<usize>
    {
        Ok(self.write(&val.to_le_bytes())?)
    }
}

impl<W: Write> ColorWriteExt for W {
    
}