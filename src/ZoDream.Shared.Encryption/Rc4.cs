using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;
using System.Security.Cryptography;

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

        public byte[] Encrypt(byte[] input)
        {
            throw new NotImplementedException();
        }

        public void Encrypt(Stream input, Stream output)
        {
            throw new NotImplementedException();
        }
    }
}
