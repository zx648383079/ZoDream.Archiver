using System;
using System.IO;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Encryption
{

    public class Rc4CipherContext: ICipherContext
    {
        public string Password { get; set; } = string.Empty;
    }

    public class Rc4 : Rc4CipherContext, IEncryptCipher, IDecryptCipher, ICipherContext
    {

        public byte[] Decrypt(byte[] input)
        {
            throw new NotImplementedException();
        }

        public void Decrypt(Stream input, Stream output)
        {
            throw new NotImplementedException();
        }

        public byte[] Decrypt(byte[] input, int inputLength)
        {
            throw new NotImplementedException();
        }

        public byte[] Encrypt(byte[] input)
        {
            throw new NotImplementedException();
        }

        public void Encrypt(Stream input, Stream output)
        {
            throw new NotImplementedException();
        }

        public byte[] Encrypt(byte[] input, int inputLength)
        {
            throw new NotImplementedException();
        }
    }
}
