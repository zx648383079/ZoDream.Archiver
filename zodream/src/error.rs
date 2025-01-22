#![allow(dead_code)]
use std::{fmt::{self, Debug, Display}, result};

use cipher::{InvalidLength, StreamCipherError};
use inout::NotEqualError;

pub type Result<T> = result::Result<T, Error>;


pub struct Error {
    message: String
}

impl Error {
    pub fn new(message: String) -> Self
    {
        Error{message}
    }

    pub fn form_error<T: std::error::Error>(error: T) -> Self 
    {
        Error::new(error.to_string())
    }

    pub fn form_str(value: &str) -> Self 
    {
        Error::new(String::from(value))
    }

}

impl Display for Error {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        fmt::Debug::fmt(&self.message, f)
    }
}

impl Debug for Error {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        fmt::Debug::fmt(&self.message, f)
    }
}

impl std::error::Error for Error {}

impl From<String> for Error {
    fn from(value: String) -> Self {
        Self::new(value)
    }
}

impl From<&str> for Error {
    fn from(value: &str) -> Self {
        Self::new(String::from(value))
    }
}

impl From<std::io::Error> for Error {
    fn from(value: std::io::Error) -> Self {
        Self::form_error(value)
    }
}

impl From<::lzxd::DecompressError> for Error {
    fn from(value: ::lzxd::DecompressError) -> Self {
        Self::form_error(value)
    }
}

impl From<NotEqualError> for Error {
    fn from(value: NotEqualError) -> Self {
        Self::new(value.to_string())
    }
}

impl From<StreamCipherError> for Error {
    fn from(value: StreamCipherError) -> Self {
        Self::new(value.to_string())
    }
}

impl From<InvalidLength> for Error {
    fn from(value: InvalidLength) -> Self {
        Self::new(value.to_string())
    }
}
