using System.IO;

namespace ZoDream.Shared.Interfaces
{
    public interface ICipherContext
    {

    }
    public interface IEncryptCipher
    {
        public byte[] Encrypt(byte[] input);
        public byte[] Encrypt(byte[] input, int inputLength);

        public void Encrypt(Stream input, Stream output);
    }
    public interface IDecryptCipher
    {
        public byte[] Decrypt(byte[] input);
        public byte[] Decrypt(byte[] input, int inputLength);

        public void Decrypt(Stream input, Stream output);
    }
}
