pub trait Encryptor {
    fn encrypt() -> u64;
    fn decrypt() -> u64;
}

// let test = "0000000000000000";
// let test = test.as_bytes();

// println!("test: {:?}", test);

// let key = GenericArray::from([48u8; 16]);
// let mut block = GenericArray::from([48u8; 16]);
// // Initialize cipher
// let cipher = Aes128::new(&key);
// // Encrypt block in-place
// println!("明文: {:?}", block);
// cipher.encrypt_block(&mut block);
// println!("密文: {:?}", base64::encode(block));

// cipher.decrypt_block(&mut block);
// println!("明文: {:?}", std::str::from_utf8(&block));