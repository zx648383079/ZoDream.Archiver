
// #[no_mangle]
// pub extern "C" fn call_from_c() {
//     println!("Just called a Rust function from C!");
// }

use ::safer_ffi::prelude::*;


#[derive_ReprC]
#[repr(C)] 
pub struct Point {
    x: i32,
    y: i32,
}


#[ffi_export]
fn origin () -> Point
{
    Point { x: 0, y: 0 }
}

#[ffi_export]
fn logger() {
    println!("Just called a Rust function from C!");
}

#[ffi_export]
fn add(a: u32, b: u32) -> u32 {
    a + b
}

#[ffi_export]
fn get_str() -> repr_c::Box<String> {
    Box::new(String::from("…")).into()
}



#[ffi_export]
fn append(fst: char_p::Ref<'_>) -> repr_c::Box<String> {
    let mut data = String::from("aa".to_owned());
    data.push_str(fst.to_str());
    Box::new(data).into()
}

#[ffi_export]
fn returns_a_fn_ptr ()
  -> extern "C" fn(u8) -> u16
{
    extern "C"
    fn f (n: u8)
      -> u16
    {
        (n as u16) << 8
    }

    f
}

#[ffi_export]
fn call_in_the_background (
    f: repr_c::Arc<dyn Send + Sync + Fn()>,
)
{
    let f2 = f.clone();
    ::std::thread::spawn(move || {
        f2.call()
    });
    drop(f);
}

#[ffi_export]
fn concat (
    fst: char_p::Ref<'_>,
    snd: char_p::Ref<'_>,
) -> char_p::Box
{
    format!("{}{}", fst.to_str(), snd.to_str())
        .try_into()
        .unwrap()
}

/// Frees a string created by `concat`.
#[ffi_export]
fn free_char_p (_string: Option<char_p::Box>)
{}

#[ffi_export]
fn with_concat (
    fst: char_p::Ref<'_>,
    snd: char_p::Ref<'_>,
    mut cb: ::safer_ffi::closure::RefDynFnMut1<'_, (), char_p::Raw>,
)
{
    let concat = concat(fst, snd);
    cb.call(concat.as_ref().into());
}

#[ffi_export(rename = "max")]
fn max_but_with_a_weird_rust_name<'a> (
    xs: c_slice::Ref<'a, i32>,
) -> Option<&'a i32>
{
    xs.as_slice()
        .iter()
        .max()
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