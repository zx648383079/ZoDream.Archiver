
// #[no_mangle]
// pub extern "C" fn call_from_c() {
//     println!("Just called a Rust function from C!");
// }

use std::{ffi::CStr, slice};

#[repr(C)]
pub
struct Point {
    x: i32,
    y: i32,
}

#[no_mangle] pub extern "C" fn origin () -> Point
{
    Point { x: 0, y: 0 }
}

#[no_mangle]
pub extern fn logger() {
    println!("Just called a Rust function from C!");
}
pub extern fn add(a: u32, b: u32) -> u32 {
    a + b
}

pub extern fn get_str() -> String {
    "aa".to_owned()
}

pub extern fn append(text: * const u8, text_len: u32) -> String {
    let slice = unsafe { slice::from_raw_parts(text, text_len as usize) };
    let cstr = unsafe { CStr::from_bytes_with_nul_unchecked(slice) };
    let mut data = String::from("aa".to_owned());
    match cstr.to_str() {
        Ok(s) => {
            data.push_str(s);
        }
        Err(_) => {

        }
    }
    
    
    data
}