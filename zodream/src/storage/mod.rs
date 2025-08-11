#[allow(dead_code)]
// pub mod lua;

pub mod reader;
pub mod writer;

pub enum EndianType
{
    LittleEndian,
    BigEndian,
}