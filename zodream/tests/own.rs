use zodream::encryption::{own::OwnEncryptor, Decryptor, Encryptor};

#[test]
fn test_own() {
    let key = b"helle";
    let input = b"aaafkdnkjank";
    let mut encrypted = vec![0u8; input.len()];
    let mut decrypted = vec![0u8; input.len()];
    let mut instance = OwnEncryptor::new(key);
    _ = instance.encrypt(input, encrypted.as_mut_slice());
    _ = instance.decrypt(encrypted.as_slice(), decrypted.as_mut_slice());
    assert_eq!(input, decrypted.as_slice());
}