use std::io::{self, Read, Write};

use encryption::threeway::ThreeWay;
use lz4::EncoderBuilder;
// use cipher::{KeyInit};
use ::safer_ffi::prelude::*;
// use ::blowfish::Blowfish;

mod encryption;

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
    _log: extern "C" fn(char_p::Raw),
    progress: extern "C" fn(u32, u32, char_p::Raw),
}


#[derive_ReprC]
#[repr(i8)]
pub enum CompressionID {
    Unkown,
    Lz4,
}

#[derive_ReprC]
#[repr(i8)]
pub enum EncryptionID {
    Unkown,
    Blowfish,
    ThreeWay,
}


#[derive_ReprC]
#[repr(opaque)]
pub struct CompressorBox {
    id: CompressionID,
    // instance: Option<Box<dyn Read>>,
}

#[ffi_export]
fn find_compressor (id: CompressionID) -> repr_c::Box<CompressorBox>
{
    Box::new(CompressorBox {id: id.into()})
        .into()
}

#[ffi_export]
fn compress_compressor (ctor: &'_ CompressorBox, input: &'_ mut InputStream, output: &'_ mut OutputStream) -> u32
{
    match ctor.id {
        CompressionID::Lz4 => {
            let mut encoder = EncoderBuilder::new()
                    .level(4)
                    .build(output).unwrap();
            let res = io::copy(input, &mut encoder);
            _ = encoder.finish();
            match res {
                Ok(i) => i as u32,
                Err(_) => 0
            }
        },
        CompressionID::Unkown => 0
    }
    // ctor.id.into_i8() as i32
}

#[ffi_export]
fn decompress_compressor (ctor: &'_ CompressorBox, input: &'_ mut InputStream, output: &'_ mut OutputStream) -> u32
{
    match ctor.id {
        CompressionID::Lz4 => {
            let mut decoder = lz4::Decoder::new(input).unwrap();
            let res = io::copy(&mut decoder, output);
            match res {
                Ok(i) => i as u32,
                Err(_) => 0
            }
        },
        CompressionID::Unkown => 0
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
    match &ctor.key {
        Some(key) => {
            match ctor.id {
                EncryptionID::Blowfish => {
                    let mut instance = Blowfish::new(key.as_slice());
                    encryption::encrypt_stream(&mut instance, input, output) as u32
                },
                EncryptionID::ThreeWay => {
                    let mut instance = ThreeWay::new(key.as_slice());
                    encryption::encrypt_stream(&mut instance, input, output) as u32
                },
                EncryptionID::Unkown => {
                    let mut instance = own::OwnEncryptor::new(key.as_slice());
                    let res = encryption::encrypt_stream(&mut instance, input, output) as u32;
                    (logger.progress)(res, key.len() as u32, char_p::new("finish").as_ref().into());
                    res
                },
                // _ => 0
            }
        },
        None => {
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
    output: &'_ mut OutputStream, ) -> u32
{
    match &ctor.key {
        Some(key) => {
            match ctor.id {
                EncryptionID::Blowfish => {
                    let mut instance = Blowfish::new(key.as_slice());
                    encryption::decrypt_stream(&mut instance, input, output) as u32
                },
                EncryptionID::ThreeWay => {
                    let mut instance = ThreeWay::new(key.as_slice());
                    encryption::decrypt_stream(&mut instance, input, output) as u32
                },
                EncryptionID::Unkown => {
                    let mut instance = own::OwnEncryptor::new(key.as_slice());
                    let res = encryption::decrypt_stream(&mut instance, input, output) as u32;
                    // (logger.progress)(res, key.len() as u32, char_p::new("finish").as_ref().into());
                    res
                },
                // _ => 0
            }
        },
        None => {
            0
        }
    }
}

#[ffi_export]
fn free_encryptor (ctor: Option<repr_c::Box<EncryptorBox>>)
{
    drop(ctor)
}


#[ffi_export]
fn lz4_decompress(
    input: c_slice::Ref<'_, u8>, 
    output: c_slice::Mut<'_, u8>,
    mut cb: ::safer_ffi::closure::RefDynFnMut2<'_, (), usize, char_p::Raw>,
    ) -> u32
{
    // match input.first() {
    //     None => {},
    //     Some(val) => cb.call(*val as usize, char_p::new("11111").as_ref().into())
    // }
    // let mut cursor: Cursor<&mut [u8]> = Cursor::new(output.as_slice());
    let mut decoder = lz4::Decoder::new(input.as_slice()).unwrap();
    let len = std::io::Read::read_to_end(&mut decoder, &mut output.as_slice().to_vec());
    // const BUFFER_SIZE: usize = 32 * 1024;
    // loop {
    //     let mut buffer = [0; BUFFER_SIZE];
    //     match decoder.read(&mut buffer) {
    //         Ok(size) => {
    //             cb.call(size);
    //             if size == 0 {
    //                 break;
    //             }
    //             cursor.write(&buffer[0..size]).unwrap();
    //         }
    //         Err(_) => {}
    //     }
    // }
    // let _ = std::io::copy(&mut decoder, &mut cursor);
    cb.call(input.len(), char_p::new("111711").as_ref().into());
    match len {
        Ok(size) => size as u32,
        Err(err) => {
            cb.call(input.len(), char_p::new(err.to_string()).as_ref().into());
            0
        }
    }
}