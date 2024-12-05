use std::fs::{self, File};
use std::io::{Cursor, Read};

use io::ByteReadExt;

use crate::storage::lua::parse;

mod encryption;
mod compression;
mod drawing;
mod error;
mod storage;
mod io;

fn main() {

    // let mut buffer = File::open("tests/lua51/concat.lua").unwrap();
    let buffer= fs::read("tests/luajit/float.luac").unwrap();
    // let mut cur = Cursor::new(&buffer);
    // let mut buf = [0;4];
    // let res = cur.read_exact(&mut buf);
    // let mut text = String::new();
    // let text = String::from_utf8_lossy(&buffer);
    // buffer.read_to_string(&mut text).unwrap();
    let parsed = parse(&buffer);
    println!("Hello, world!");
}