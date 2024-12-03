using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal class Mr0kCipherContext: ICipherContext
    {
        public byte[] ExpansionKey { get; set; } = [];
        public byte[] SBox { get; set; } = [];
        public byte[] InitVector { get; set; } = [];
        public byte[] BlockKey { get; set; } = [];
        public byte[] PostKey { get; set; } = [];
    }

    internal class Mr0kCipher : Mr0kCipherContext, IDecryptCipher, ICipherContext
    {
        private const int BlockSize = 0x400;

        private static readonly byte[] _mr0kMagic = [0x6D, 0x72, 0x30, 0x6B];

        public static bool IsSupport(byte[] input) => input[..4].SequenceEqual(_mr0kMagic);

        public byte[] Decrypt(byte[] input)
        {
            var key1 = new byte[0x10];
            var key2 = new byte[0x10];
            var key3 = new byte[0x10];

            Array.Copy(input, 4, key1, 0, 0x10);
            Array.Copy(input, 0x74, key2, 0, 0x10);
            Array.Copy(input, 0x84, key3, 0, 0x10);

            var encryptedBlockSize = Math.Min(0x10 * ((input.Length - 0x94) >> 7), BlockSize);

            if (InitVector is not null)
            {
                for (int i = 0; i < InitVector.Length; i++)
                {
                    key2[i] ^= InitVector[i];
                }
            }

            if (SBox is not null && SBox.Length > 0)
            {
                for (int i = 0; i < 0x10; i++)
                {
                    key1[i] = SBox[(i % 4 * 0x100) | key1[i]];
                }
            }

            using var aes = Aes.Create();
            aes.Key = ExpansionKey;
            key1 = aes.DecryptEcb(key1, PaddingMode.None);
            key3 = aes.DecryptEcb(key3, PaddingMode.None);

            for (var i = 0; i < key1.Length; i++)
            {
                key1[i] ^= key3[i];
            }

            Array.Copy(key1, 0, input, 0x84, 0x10);

            var seed1 = BinaryPrimitives.ReadUInt64LittleEndian(key2);
            var seed2 = BinaryPrimitives.ReadUInt64LittleEndian(key3);
            var seed = seed2 ^ seed1 ^ (seed1 + (uint)input.Length - 20);

            var encryptedBlock = input.Skip(0x94).Take(encryptedBlockSize).ToArray();
            var seedSpan = BitConverter.GetBytes(seed);
            for (var i = 0; i < encryptedBlockSize; i++)
            {
                encryptedBlock[i] ^= (byte)(seedSpan[i % seedSpan.Length] ^ BlockKey[i % BlockKey.Length]);
            }

            var res = input[0x14..];

            if (PostKey is not null && PostKey.Length > 0)
            {
                for (int i = 0; i < 0xC00; i++)
                {
                    res[i] ^= PostKey[i % PostKey.Length];
                }
            }

            return res;
        }

        public void Decrypt(Stream input, Stream output)
        {
            var buffer = input.ToArray();
            var res = Decrypt(buffer);
            output.Write(res);
        }
    }
}
