use zodream::encryption::{arc4::Arc4, Decryptor, Encryptor};

// #[test]
// fn test_arc4() {
//     let key = b"hellewordaaa";
//     let input = b"aaafkdnkjank";
//     let mut encrypted = vec![0u8; input.len()];
//     let mut decrypted = vec![0u8; input.len()];
//     let mut instance1 = Arc4::new(key);
//     let mut instance2 = Arc4::new(key);
//     _ = instance1.encrypt(input, encrypted.as_mut_slice());
//     _ = instance2.decrypt(encrypted.as_slice(), decrypted.as_mut_slice());
//     assert_eq!(input, decrypted.as_slice());
// }