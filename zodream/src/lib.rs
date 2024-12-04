use std::io::{Read, Write};

use compression::lz4::Lz4Compressor;
use compression::lzxd::LzxdCompressor;
use compression::{Compressor, Decompressor};
use drawing::astc::AstcDecoder;
use drawing::bcn::BcnDecoder;
use drawing::crunch::CrunchDecoder;
use drawing::decode_stream;
use drawing::etc::EtcDecoder;
use drawing::pvr::PvrDecoder;
use encryption::arc4::Arc4;
use encryption::threeway::ThreeWay;
use drawing::atc::AtcDecoder;
use ::safer_ffi::prelude::*;

mod encryption;
mod compression;
mod drawing;
mod error;
mod storage;
mod io;

use error::{Error, Result};
use encryption::blowfish::Blowfish;
use encryption::own;


#[derive_ReprC]
#[repr(opaque)]
pub struct InputStream {
    // read: extern "C" fn(c_slice::Ref<'_, u8>) -> u32,
    read: extern "C" fn(* mut u8, u32) -> usize,
}


impl Read for InputStream {
    fn read(&mut self, buf: &mut [u8]) -> std::io::Result<usize> {
        if buf.is_empty() {
            return Ok(0);
        }
        let size = (self.read)(buf.as_mut_ptr(), buf.len() as u32);
        return Ok(size);
    }
}

#[derive_ReprC]
#[repr(opaque)]
pub struct OutputStream {
    // write: extern "C" fn(c_slice::Ref<'_, u8>, u32),
    write: extern "C" fn(* const u8, u32),
}

impl Write for OutputStream {
    fn write(&mut self, buf: &[u8]) -> std::io::Result<usize> {
        if buf.is_empty() {
            return Ok(0);
        }
        let size = buf.len();
        (self.write)(buf.as_ptr(), size as u32);
        Ok(size)
    }
    fn flush(&mut self) -> std::io::Result<()> {
        Ok(())
    }
}

#[derive_ReprC]
#[repr(opaque)]
pub struct Logger {
    log: extern "C" fn(char_p::Raw),
    _progress: extern "C" fn(u32, u32, char_p::Raw),
}


#[derive_ReprC]
#[repr(i8)]
pub enum CompressionID {
    Unknown,
    Lz4,
    Lzxd,
    Lua,
}

#[derive_ReprC]
#[repr(i8)]
pub enum EncryptionID {
    Unknown,
    Blowfish,
    BlowfishCBC,
    ThreeWay,
    Arc4,
}

#[derive_ReprC]
#[repr(i8)]
pub enum PixelID {
    Unknown,
    AtcRgb,
    AtcRgba,
    AsTcHdr,
    AsTcRgb,
    AsTcRgba,
    Bcn,
    EtcRgb,
    EtcRgba,
    EacR,
    EacRg,
    PvrTcRgb,
    PvrTcRgba,
    Crunch,
    UnityCrunch,
}



#[derive_ReprC]
#[repr(opaque)]
pub struct CompressorBox {
    id: CompressionID,
    // instance: Option<Box<dyn Read>>,
}

impl CompressorBox {
    fn compress<R, W>(&self, input: & mut R, output: & mut W) -> Result<usize>
        where R : Read, W : Write
    {
        match self.id {
            CompressionID::Lz4 => {
                let mut instance = Lz4Compressor::new();
                instance.compress(input, output)
            },
            CompressionID::Lzxd => {
                Ok(0)
            },
            CompressionID::Unknown | CompressionID::Lua => Ok(0)
        }
    }
    fn decompress<R, W>(&self, input: & mut R, output: & mut W) -> Result<usize>
        where R : Read, W : Write
    {
        match self.id {
            CompressionID::Lz4 => {
                let mut instance = Lz4Compressor::new();
                instance.decompress(input, output)
            },
            CompressionID::Lzxd => {
                let mut instance = LzxdCompressor::new();
                instance.decompress(input, output)
            },
            CompressionID::Lua => {
                let mut instance = LzxdCompressor::new();
                instance.decompress(input, output)
            },
            CompressionID::Unknown => Ok(0)
        }
    }
}

#[ffi_export]
fn find_compressor (id: CompressionID) -> repr_c::Box<CompressorBox>
{
    Box::new(CompressorBox {id: id.into()})
        .into()
}

#[ffi_export]
fn compress_compressor (
    ctor: &'_ CompressorBox, 
    input: &'_ mut InputStream, 
    output: &'_ mut OutputStream, 
    logger: &'_ Logger) -> u64
{
    let res = ctor.compress(input, output);
    match res {
        Ok(i) => i as u64,
        Err(err) => {
            (logger.log)(char_p::new(err.to_string()).as_ref().into());
            0
        }
    }
}

#[ffi_export]
fn decompress_compressor(
    ctor: &'_ CompressorBox, 
    input: &'_ mut InputStream, 
    output: &'_ mut OutputStream, 
    logger: &'_ Logger) -> u64
{
    let res = ctor.decompress(input, output);
    match res {
        Ok(i) => i as u64,
        Err(err) => {
            (logger.log)(char_p::new(err.to_string()).as_ref().into());
            0
        }
    }
}

#[ffi_export]
fn free_compressor (ctor: Option<repr_c::Box<CompressorBox>>)
{
    drop(ctor)
}

#[derive_ReprC]
#[repr(opaque)]
pub struct EncryptorBox {
    id: EncryptionID,
    key: Option<Vec<u8>>,
    // instance: Option<Box<dyn Encryptor>>,
}

impl EncryptorBox {
    fn encrypt<R, W>(&self, input: & mut R, output: & mut W) -> Result<usize>
        where R : Read, W : Write
    {
        match &self.key {
            Some(key) => {
                match self.id {
                    EncryptionID::Blowfish => {
                        let mut instance = Blowfish::new(key.as_slice())?;
                        encryption::encrypt_stream(&mut instance, input, output)
                    },
                    EncryptionID::BlowfishCBC => {
                        let mut instance = Blowfish::new_cbc_enc(key.as_slice())?;
                        encryption::encrypt_stream(&mut instance, input, output)
                    },
                    EncryptionID::ThreeWay => {
                        let mut instance = ThreeWay::new(key.as_slice());
                        encryption::encrypt_stream(&mut instance, input, output)
                    },
                    EncryptionID::Arc4 => {
                        let mut instance = Arc4::new(key.as_slice());
                        encryption::encrypt_stream(&mut instance, input, output)
                    },
                    EncryptionID::Unknown => {
                        let mut instance = own::OwnEncryptor::new(key.as_slice());
                        encryption::encrypt_stream(&mut instance, input, output)
                    },
                    // _ => 0
                }
            },
            None => Err(Error::form_str("key error"))
        }
    }

    fn decrypt<R, W>(&self, input: & mut R, output: & mut W) -> Result<usize>
        where R : Read, W : Write
    {
        match &self.key {
            Some(key) => {
                match self.id {
                    EncryptionID::Blowfish => {
                        let mut instance = Blowfish::new(key.as_slice())?;
                        encryption::decrypt_stream(&mut instance, input, output)
                    },
                    EncryptionID::BlowfishCBC => {
                        let mut instance = Blowfish::new_cbc_dec(key.as_slice())?;
                        encryption::decrypt_stream(&mut instance, input, output)
                    },
                    EncryptionID::ThreeWay => {
                        let mut instance = ThreeWay::new(key.as_slice());
                        encryption::decrypt_stream(&mut instance, input, output)
                    },
                    EncryptionID::Arc4 => {
                        let mut instance = Arc4::new(key.as_slice());
                        encryption::decrypt_stream(&mut instance, input, output)
                    },
                    EncryptionID::Unknown => {
                        let mut instance = own::OwnEncryptor::new(key.as_slice());
                        encryption::decrypt_stream(&mut instance, input, output)
                    },
                    // _ => 0
                }
            },
            None => Err(Error::form_str("key error"))
        }
    }
}


#[ffi_export]
fn find_encryptor (id: EncryptionID) -> repr_c::Box<EncryptorBox>
{
    Box::new(EncryptorBox {id: id.into(), key: None})
        .into()
}

#[ffi_export]
fn find_encryptor_with_key (id: EncryptionID, key: char_p::Ref<'_>) -> repr_c::Box<EncryptorBox>
{
    // let instance: Option<Box<dyn Encryptor>> = match id {
    //     EncryptionID::Blowfish => {
    //         Some(Box::new(Blowfish::new(key.to_bytes())))
    //     },
    //     EncryptionID::Unkown => {
    //         Some(Box::new(own::OwnEncryptor::new(key.to_bytes())))
    //     },
    //     _ => None
    // };
    Box::new(EncryptorBox {id: id.into(), key: Some(key.to_bytes().to_owned())})
        .into()
}

#[ffi_export]
fn encrypt_encryptor (
    ctor: &'_ EncryptorBox, 
    input: &'_ mut InputStream, 
    output: &'_ mut OutputStream, 
    logger: &'_ Logger,
) -> u32
{
    let res = ctor.encrypt(input, output);
    match res {
        Ok(i) => i as u32,
        Err(err) => {
            (logger.log)(char_p::new(err.to_string()).as_ref().into());
            0
        }
    }
    // const BLOCK_SIZE: usize = 1024;
    // let mut len = 0;
    // let mut buffer: [u8; BLOCK_SIZE] = [0; BLOCK_SIZE];
    // let mut l = BLOCK_SIZE;
    // while l == BLOCK_SIZE {
    //     (logger.log)(char_p::new("entry").as_ref().into());
    //     let c = (input.read)(buffer.as_mut_ptr(), BLOCK_SIZE as u32);
    //     (logger.progress)(c.try_into().unwrap(), 100, char_p::new("finish").as_ref().into());
    //     l = c as usize;
    //     for i in 0..l {
    //         if buffer[i] > 128 {
    //             buffer[i] -= 9
    //         } else {
    //             buffer[i] += 9
    //         }
    //     }
    //     (output.write)(buffer.as_ptr(), l as u32);
    //     len += l;
    // }
    // len as u32
}

#[ffi_export]
fn decrypt_encryptor (
    ctor: &'_ EncryptorBox,     
    input: &'_ mut InputStream, 
    output: &'_ mut OutputStream, 
    logger: &'_ Logger) -> u32
{
    let res = ctor.decrypt(input, output);
    match res {
        Ok(i) => i as u32,
        Err(err) => {
            (logger.log)(char_p::new(err.to_string()).as_ref().into());
            0
        }
    }
}

#[ffi_export]
fn free_encryptor (ctor: Option<repr_c::Box<EncryptorBox>>)
{
    drop(ctor)
}

#[derive_ReprC]
#[repr(opaque)]
pub struct PainterBox {
    id: PixelID,
    width: u32,
    height: u32,
    block_width: u32,
    block_heihgt: u32,
}

impl PainterBox {
    fn encode<R, W>(&self, _input: & mut R, _output: & mut W) -> Result<usize>
        where R : Read, W : Write
    {
        match self.id {
            _ => Ok(0)
        }
    }

    fn decode<R, W>(&self, input: & mut R, output: & mut W) -> Result<usize>
        where R : Read, W : Write
    {
        match self.id {
            PixelID::AtcRgb | PixelID::AtcRgba => {
                let mut instance = AtcDecoder::new(self.block_width as u8);
                decode_stream(&mut instance, input, self.width, self.height, output)
            },
            PixelID::AsTcHdr | PixelID::AsTcRgb | PixelID::AsTcRgba => {
                let mut instance = AstcDecoder::new(self.block_width as usize, self.block_heihgt as usize);
                decode_stream(&mut instance, input, self.width, self.height, output)
            },
            PixelID::Bcn => {
                let mut instance = BcnDecoder::new(self.block_width as u8);
                decode_stream(&mut instance, input, self.width, self.height, output)
            },
            PixelID::EtcRgb | PixelID::EtcRgba => {
                let mut instance = EtcDecoder::new(self.block_width as u8, self.block_heihgt as u8);
                decode_stream(&mut instance, input, self.width, self.height, output)
            },
            PixelID::EacR => {
                let mut instance = EtcDecoder::new_signed(3, 1, self.block_width == 1);
                decode_stream(&mut instance, input, self.width, self.height, output)
            },
            PixelID::EacRg => {
                let mut instance = EtcDecoder::new_signed(3, 2, self.block_width == 1);
                decode_stream(&mut instance, input, self.width, self.height, output)
            },
            PixelID::PvrTcRgb | PixelID::PvrTcRgba => {
                let mut instance = PvrDecoder::new(self.block_width as u8);
                decode_stream(&mut instance, input, self.width, self.height, output)
            },
            PixelID::Crunch => {
                let mut instance = CrunchDecoder::new(1);
                decode_stream(&mut instance, input, self.width, self.height, output)
            },
            PixelID::UnityCrunch => {
                let mut instance = CrunchDecoder::new(2);
                decode_stream(&mut instance, input, self.width, self.height, output)
            },
            _ => Ok(0)
        }
    }
}

#[ffi_export]
fn find_painter (id: PixelID, width: u32, height: u32, bw: u32, bh: u32) -> repr_c::Box<PainterBox>
{
    Box::new(PainterBox {id: id.into(), width, height, block_width: bw, block_heihgt: bh})
        .into()
}

#[ffi_export]
fn encode_painter (  
    ctor: &'_ PainterBox,    
    input: &'_ mut InputStream, 
    output: &'_ mut OutputStream, 
    logger: &'_ Logger) -> u32
{
    let res = ctor.encode(input, output);
    match res {
        Ok(i) => i as u32,
        Err(err) => {
            (logger.log)(char_p::new(err.to_string()).as_ref().into());
            0
        }
    }
}

#[ffi_export]
fn decode_painter (  
    ctor: &'_ PainterBox,    
    input: &'_ mut InputStream, 
    output: &'_ mut OutputStream, 
    logger: &'_ Logger) -> u32
{
    let res = ctor.decode(input, output);
    match res {
        Ok(i) => i as u32,
        Err(err) => {
            (logger.log)(char_p::new(err.to_string()).as_ref().into());
            0
        }
    }
}

#[ffi_export]
fn free_painter (ctor: Option<repr_c::Box<PainterBox>>)
{
    drop(ctor)
}


#[cfg(test)]
mod tests {
    use std::fs::File;
    use super::*;

    
    #[test]
    fn test_decrypt() {
        //File::options().read(true).write(true).open();
        let mut input = File::open("texture_00.ktx").unwrap();
        let length = input.metadata().unwrap().len();
        //input.set_len(input.metadata().unwrap().len() - 8).unwrap();
        let mut output = File::create("texture.png").unwrap();
        let key = char_p::new("");
        let instance = find_encryptor_with_key(EncryptionID::BlowfishCBC, key.as_ref());
        let res = instance.decrypt(&mut input, &mut output).unwrap();
        assert_eq!(res, length as usize);
    }
}