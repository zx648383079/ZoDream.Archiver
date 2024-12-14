using System;
using System.Buffers;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Unity.Scanners
{

    internal class CNCipherContext: ICipherContext
    {
        public byte[] Index { get; set; } = new byte[0x10];
        public byte[] Sub { get; set; } = new byte[0x10];
        /// <summary>
        /// Convert.FromHexString("")
        /// </summary>
        public byte[] Key { get; set; } = [];
    }

    internal class CNCipher : CNCipherContext, IDecryptCipher, ICipherContext
    {
        private const string Signature = "#$unity3dchina!@";

        public CNCipher()
        {
            
        }

        public CNCipher(IBundleBinaryReader reader)
        {
            reader.ReadUInt32();

            var infoBytes = reader.ReadBytes(0x10);
            var infoKey = reader.ReadBytes(0x10);
            reader.Position += 1;

            var signatureBytes = reader.ReadBytes(0x10);
            var signatureKey = reader.ReadBytes(0x10);
            reader.Position += 1;

            DecryptKey(signatureKey, signatureBytes);

            var str = Encoding.UTF8.GetString(signatureBytes);
            if (str != Signature)
            {
                throw new Exception($"Invalid Signature, Expected {Signature} but found {str} instead");
            }

            DecryptKey(infoKey, infoBytes);

            infoBytes = infoBytes.To4bArray();
            infoBytes.AsSpan(0, 0x10).CopyTo(Index);
            var subBytes = infoBytes.AsSpan(0x10, 0x10);
            for (var i = 0; i < subBytes.Length; i++)
            {
                var idx = (i % 4 * 4) + (i / 4);
                Sub[idx] = subBytes[i];
            }
        }

        private ICryptoTransform CreateEncryptor()
        {
            using var aes = Aes.Create();
            aes.Mode = CipherMode.ECB;
            aes.Key = Key;
            return aes.CreateEncryptor();
        }

        public virtual void DecryptBlock(Span<byte> bytes, int size, int index)
        {
            var offset = 0;
            while (offset < size)
            {
                offset += Decrypt(bytes[offset..], index++, size - offset);
            }
        }

        private void DecryptKey(byte[] key, byte[] data)
        {
            if (Key is null || Key.Length == 0)
            {
                return;
            }
            key = CreateEncryptor().TransformFinalBlock(key, 0, key.Length);
            for (int i = 0; i < 0x10; i++)
            {
                data[i] ^= key[i];
            }
        }

        protected virtual int DecryptByte(Span<byte> bytes, ref int offset, ref int index)
        {
            var b = Sub[((index >> 2) & 3) + 4] + Sub[index & 3] + Sub[((index >> 4) & 3) + 8] + Sub[((byte)index >> 6) + 12];
            bytes[offset] = (byte)((Index[bytes[offset] & 0xF] - b) & 0xF | 0x10 * (Index[bytes[offset] >> 4] - b));
            b = bytes[offset];
            offset++;
            index++;
            return b;
        }

        protected int Decrypt(Span<byte> bytes, int index, int remaining)
        {
            var offset = 0;

            var curByte = DecryptByte(bytes, ref offset, ref index);
            var byteHigh = curByte >> 4;
            var byteLow = curByte & 0xF;

            if (byteHigh == 0xF)
            {
                int b;
                do
                {
                    b = DecryptByte(bytes, ref offset, ref index);
                    byteHigh += b;
                } while (b == 0xFF);
            }

            offset += byteHigh;

            if (offset < remaining)
            {
                DecryptByte(bytes, ref offset, ref index);
                DecryptByte(bytes, ref offset, ref index);
                if (byteLow == 0xF)
                {
                    int b;
                    do
                    {
                        b = DecryptByte(bytes, ref offset, ref index);
                    } while (b == 0xFF);
                }
            }

            return offset;
        }

        public byte[] Decrypt(byte[] input)
        {
            return Decrypt(input, input.Length);
        }
        public byte[] Decrypt(byte[] input, int inputLength)
        {
            var length = Decrypt(input, 0, inputLength);
            return input[..length];
        }

        public void Decrypt(Stream input, Stream output)
        {
            var length = (int)(input.Length - input.Position);
            var buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                input.ReadExactly(buffer, 0, length);
                var res = Decrypt(buffer, 0, length);
                output.Write(buffer, 0, res);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }
    }
}
