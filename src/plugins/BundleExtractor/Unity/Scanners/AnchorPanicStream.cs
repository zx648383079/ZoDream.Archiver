using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal class AnchorPanicStream(Stream input, string fullPath) : DeflateStream(input)
    {
        const int BlockSize = 0x800;

        private readonly byte[] _keys = GetKey(Path.GetFileNameWithoutExtension(fullPath));

        public long RemainingLength => Length - Position;

        public override int Read(byte[] buffer, int offset, int count)
        {
            var pos = Position;
            if (count == 0 || RemainingLength <= 0)
            {
                return 0;
            }
            var chunkBegin = (int)Math.Floor((double)pos / BlockSize);
            var end = pos + count;
            var chunkEnd = (int)Math.Ceiling((double)end / BlockSize);
            var sent = 0;
            for (int i = chunkBegin; i < chunkEnd; i++)
            {
                var begin = i * BlockSize;
                Seek(begin, SeekOrigin.Begin);
                var chunkSize = (int)Math.Min(RemainingLength, BlockSize);
                if (chunkSize <= 0)
                {
                    break;
                }
                var chunk = input.ReadBytes(chunkSize);
                if (IsEncrypt((int)Length, i))
                {
                    RC4(chunk, _keys);
                }
                var p = (int)Math.Max(0, pos - begin);
                var c = Math.Min(count - sent, chunk.Length - p);
                Array.Copy(chunk, p, buffer, offset + sent, c);
                sent += c;
            }
            return sent;
        }


        private static bool IsEncrypt(int fileSize, int chunkIndex)
        {
            const int MaxEncryptChunkIndex = 4;

            if (chunkIndex == 0)
            {
                return true;
            }

            if (fileSize / BlockSize == chunkIndex)
            {
                return true;
            }

            if (MaxEncryptChunkIndex < chunkIndex)
            {
                return false;
            }

            return fileSize % 2 == chunkIndex % 2;
        }

        private static void RC4(Span<byte> data, byte[] key)
        {
            var S = new int[0x100];
            for (int _ = 0; _ < 0x100; _++)
            {
                S[_] = _;
            }

            var T = new int[0x100];

            if (key.Length == 0x100)
            {
                Buffer.BlockCopy(key, 0, T, 0, key.Length);
            }
            else
            {
                for (int _ = 0; _ < 0x100; _++)
                {
                    T[_] = key[_ % key.Length];
                }
            }

            int i = 0;
            int j = 0;
            for (i = 0; i < 0x100; i++)
            {
                j = (j + S[i] + T[i]) % 0x100;

                (S[j], S[i]) = (S[i], S[j]);
            }

            i = j = 0;
            for (int iteration = 0; iteration < data.Length; iteration++)
            {
                i = (i + 1) % 0x100;
                j = (j + S[i]) % 0x100;

                (S[j], S[i]) = (S[i], S[j]);
                var K = (uint)S[(S[j] + S[i]) % 0x100];

                data[iteration] ^= Convert.ToByte(K);
            }
        }

        private static byte[] GetKey(string fileName)
        {
            const string Key = "KxZKZolAT3QXvsUU";

            string keyHash = CalculateMD5(Key);
            string nameHash = CalculateMD5(fileName);
            var key = $"{keyHash[..5]}leiyan{nameHash[Math.Max(0, nameHash.Length - 5)..]}";
            return Encoding.UTF8.GetBytes(key);

            
        }

        private static string CalculateMD5(string str)
        {
            var bytes = Encoding.UTF8.GetBytes(str);
            bytes = MD5.HashData(bytes);
            return Convert.ToHexString(bytes).ToLowerInvariant();
        }
    }
}