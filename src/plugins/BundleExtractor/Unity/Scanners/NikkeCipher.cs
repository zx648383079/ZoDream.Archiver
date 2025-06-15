using System.IO;
using System.Security.Cryptography;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal class NikkeCipher : IDecryptCipher
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
            var reader = new BundleBinaryReader(input, EndianType.LittleEndian);
            var magic = reader.ReadStringZeroTerm(4);
            var version = reader.ReadUInt32();
            if (magic != "NKAB" || version != 1)
            {
                input.Seek(0, SeekOrigin.Begin);
                input.CopyTo(output);
                return;
            }
            var headerLen = unchecked(reader.ReadInt16() + 100);
            var encryptionMode = unchecked(reader.ReadInt16() + 100);
            var keyLen = unchecked(reader.ReadInt16() + 100);
            var encryptedLength = unchecked(reader.ReadInt16() + 100);

            var key = reader.ReadBytes(keyLen);
            var iv = reader.ReadBytes(16);

            var sha = SHA256.Create();
            var aes = Aes.Create();
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.None;
            var decryptor = aes.CreateDecryptor(sha.ComputeHash(key), iv);
            var decryptedHeader = decryptor.TransformFinalBlock(reader.ReadBytes(encryptedLength), 0, encryptedLength);

            output.Write(decryptedHeader, 0, decryptedHeader.Length);
            input.CopyTo(output);
        }
    }
}
