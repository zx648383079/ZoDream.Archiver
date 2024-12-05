use zodream::encryption::{threeway::ThreeWay, Decryptor, Encryptor};

#[test]
fn test_3way() {
    let key = b"hellewordaaa";
    let input = b"aaafkdnkjank";
    let mut encrypted = vec![0u8; input.len()];
    let mut decrypted = vec![0u8; input.len()];
    let mut instance = ThreeWay::new(key);
    // assert_eq!(instance.key, [1819043176, 1919907685, 1633771876]);
    _ = instance.encrypt(input, encrypted.as_mut_slice());
    _ = instance.decrypt(encrypted.as_slice(), decrypted.as_mut_slice());
    assert_eq!(input, decrypted.as_slice());
}