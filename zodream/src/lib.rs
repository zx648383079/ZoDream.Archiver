

// #[no_mangle]
// pub extern "C" fn call_from_c() {
//     println!("Just called a Rust function from C!");
// }

// use cipher::{KeyInit};
use ::safer_ffi::prelude::*;
// use ::blowfish::Blowfish;

#[derive_ReprC]
#[repr(opaque)]
pub struct InputStream {
    // read: extern "C" fn(c_slice::Ref<'_, u8>) -> u32,
    read: extern "C" fn(* mut u8, u32) -> usize,
}

#[derive_ReprC]
#[repr(opaque)]
pub struct OutputStream {
    // write: extern "C" fn(c_slice::Ref<'_, u8>, u32),
    write: extern "C" fn(* const u8, u32),
}

#[derive_ReprC]
#[repr(opaque)]
pub struct Logger {
    log: extern "C" fn(char_p::Raw),
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
}


#[derive_ReprC]
#[repr(opaque)]
pub struct Compressor {
    id: CompressionID,
}

#[ffi_export]
fn find_compressor (id: CompressionID) -> repr_c::Box<Compressor>
{
    Box::new(Compressor {id: id.into()})
        .into()
}

#[ffi_export]
fn compress_compressor (ctor: &'_ Compressor, _input: &'_ InputStream, _output: &'_ OutputStream) -> u32
{
    match ctor.id {
        CompressionID::Lz4 => 1,
        CompressionID::Unkown => 0
    }
    // ctor.id.into_i8() as i32
}

#[ffi_export]
fn decompress_compressor (ctor: &'_ Compressor, _input: &'_ InputStream, _output: &'_ OutputStream) -> u32
{
    match ctor.id {
        CompressionID::Lz4 => 1,
        CompressionID::Unkown => 0
    }
}

#[ffi_export]
fn free_compressor (ctor: Option<repr_c::Box<Compressor>>)
{
    drop(ctor)
}

#[derive_ReprC]
#[repr(opaque)]
pub struct Encryptor {
    id: EncryptionID,
    // instance: Blowfish,
}


#[ffi_export]
fn find_encryptor (id: EncryptionID) -> repr_c::Box<Encryptor>
{
    // let instance = Blowfish::new_from_slice("jjjjj".as_bytes()).unwrap();
    Box::new(Encryptor {id: id.into()})
        .into()
}

#[ffi_export]
fn find_encryptor_with_key (id: EncryptionID, _key: char_p::Ref<'_>) -> repr_c::Box<Encryptor>
{
    // let instance: Blowfish = Blowfish::new_from_slice(key.to_bytes()).unwrap();
    Box::new(Encryptor {id: id.into()})
        .into()
}

#[ffi_export]
fn encrypt_encryptor (
    ctor: &'_ Encryptor, 
    input: &'_ InputStream, 
    output: &'_ OutputStream, 
    logger: &'_ Logger,
) -> u32
{
    const BLOCK_SIZE: usize = 1024;
    let mut len = 0;
    let mut buffer: [u8; BLOCK_SIZE] = [0; BLOCK_SIZE];
    // let buffer_ref = c_slice::Mut::from(buffer.as_mut_slice());
    match ctor.id {
        EncryptionID::Blowfish => {},
        EncryptionID::Unkown => {
            let mut l = BLOCK_SIZE;
            while l == BLOCK_SIZE {
                (logger.log)(char_p::new("entry").as_ref().into());
                let c = (input.read)(buffer.as_mut_ptr(), BLOCK_SIZE as u32);
                (logger.progress)(c.try_into().unwrap(), 100, char_p::new("finish").as_ref().into());
                l = c as usize;
                for i in 0..l {
                    if buffer[i] > 128 {
                        buffer[i] -= 9
                    } else {
                        buffer[i] += 9
                    }
                }
                (output.write)(buffer.as_ptr(), l as u32);
                len += l;
            }
        },
    }
    len as u32
    // ctor.id.into_i8() as i32
}

#[ffi_export]
fn decrypt_encryptor (ctor: &'_ Encryptor, _input: &'_ InputStream, _output: &'_ OutputStream) -> u32
{
    match ctor.id {
        EncryptionID::Blowfish => {
            // _ = ctor.instance.clone();
            0
        },
        EncryptionID::Unkown => {

            1
        },
    }
}

#[ffi_export]
fn free_encryptor (ctor: Option<repr_c::Box<Encryptor>>)
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