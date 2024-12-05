// use std::fs::File;

// use safer_ffi::char_p;
// use zodream::{find_encryptor_with_key, EncryptionID};

// #[test]
// fn test_decrypt() {
//     //File::options().read(true).write(true).open();
//     let mut input = File::open("texture_00.ktx").unwrap();
//     let length = input.metadata().unwrap().len();
//     //input.set_len(input.metadata().unwrap().len() - 8).unwrap();
//     let mut output = File::create("texture.png").unwrap();
//     let key = char_p::new("");
//     let instance = find_encryptor_with_key(EncryptionID::BlowfishCBC, key.as_ref());
//     let res = instance.decrypt(&mut input, &mut output).unwrap();
//     assert_eq!(res, length as usize);
// }