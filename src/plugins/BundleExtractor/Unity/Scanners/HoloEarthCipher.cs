using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal class HoloEarthCipher(string fileName) : IDecryptCipher
    {
        public byte[] Decrypt(byte[] input)
        {
            return Decrypt(input, input.Length);
        }

        public byte[] Decrypt(byte[] input, int inputLength)
        {
            using var output = new MemoryStream();
            using var data = new MemoryStream(input, 0, inputLength);
            Decrypt(data, output);
            return output.ToArray();
        }

        public void Decrypt(Stream input, Stream output)
        {
            var name = Path.GetFileNameWithoutExtension(fileName);
            using var aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;
            aes.KeySize = 128;
            aes.Key = DeriveKey(name, aes.KeySize / 8);
            aes.IV = new byte[16];
            using var encryptor = aes.CreateEncryptor();

            Decrypt(encryptor, input, output, aes.BlockSize / 8);
        }

        private static void Decrypt(ICryptoTransform enc,
            Stream input, Stream output, int blockSize)
        {
            var offset = 0;
            var length = input.Length;
            var remainder = offset % blockSize;
            var quotient = offset / blockSize + 1;
            var transformBuffer = new byte[blockSize];
            var quotientBuffer = new byte[blockSize];
            while (offset < length)
            {
                if (remainder % blockSize == 0)
                {
                    var bytes = BitConverter.GetBytes(quotient++);
                    bytes.CopyTo(quotientBuffer, 0);
                    enc.TransformBlock(quotientBuffer, 0, quotientBuffer.Length, transformBuffer, 0);
                    remainder = 0;
                }
                output.WriteByte((byte)((byte)input.ReadByte() ^ transformBuffer[remainder++]));
                offset++;
            }
        }

        private static string Password = "a4886faf24895680f4af42ab802b3dc44d70e3aaccb26b9098d65fc8ff8d9184";

        private static byte[] DeriveKey(string filename, int length)
        {
            using var derive = new PasswordDeriveBytes(Password, Encoding.UTF8.GetBytes(filename));
            return derive.GetBytes(length);
        }
    }
}
