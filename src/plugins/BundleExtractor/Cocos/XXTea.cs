using System;
using System.IO;
using System.Runtime.CompilerServices;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Numerics;

namespace ZoDream.BundleExtractor.Cocos
{

    public class XXTeaCipherContext : ICipherContext
    {
        public XXTeaCipherContext(byte[] keys)
        {
            Keys = new uint[MathEx.Ceiling(keys.Length, 4)];
            Buffer.BlockCopy(keys, 0, Keys, 0, keys.Length);
        }
        public uint[] Keys { get; private set; }
    }

    public class XXTea(byte[] keys) : XXTeaCipherContext(keys), IEncryptCipher, IDecryptCipher, ICipherContext
    {
        const uint DELTA = 0x9e3779b9;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint MX(uint sum, uint y, uint z, int p, uint e, ReadOnlySpan<uint> k)
        {
            return (((z >> 5) ^ (y << 2)) + ((y >> 3) ^ (z << 4))) ^ ((sum ^ y) + (k[(p & 3) ^ (int)e] ^ z));
        }

        public byte[] Encrypt(byte[] input)
        {
            return Encrypt(input, input.Length);
        }

        public byte[] Encrypt(byte[] input, int inputLength)
        {
            throw new NotImplementedException();
        }

        public void Encrypt(Stream input, Stream output)
        {
            throw new NotImplementedException();
        }

        private static void Encrypt(Span<uint> v, ReadOnlySpan<uint> k)
        {
            int n = v.Length - 1;
            uint z = v[n];
            uint y;
            int q = 6 + 52 / v.Length;
            uint sum = 0;
            unchecked
            {
                while (q-- > 0)
                {
                    sum += DELTA;
                    uint e = (sum >> 2) & 3;
                    for (int p = 0; p <= n; p++)
                    {
                        y = v[(p + 1) % v.Length];
                        z = v[p] += MX(sum, y, z, p, e, k);
                    }
                }
            }
        }

        public byte[] Decrypt(byte[] input)
        {
            return Decrypt(input, input.Length);
        }

        public byte[] Decrypt(byte[] input, int inputLength)
        {
            throw new NotImplementedException();
        }

        public void Decrypt(Stream input, Stream output)
        {
            throw new NotImplementedException();
        }

        private static void Decrypt(Span<uint> v, ReadOnlySpan<uint> k)
        {
            int n = v.Length - 1;
            int q = 6 + 52 / v.Length;
            uint sum = unchecked((uint)q * DELTA);
            uint y = v[0];
            uint z;
            unchecked
            {
                do
                {
                    uint e = (sum >> 2) & 3;
                    for (int p = n; p >= 0; p--)
                    {
                        z = v[(p + n) % v.Length];
                        y = v[p] -= MX(sum, y, z, p, e, k);
                    }
                    sum -= DELTA;
                } while (--q > 0);
            }
        }
    }
}
