using K4os.Compression.LZ4;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using ZoDream.BundleExtractor.Unity.BundleFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public class FairGuardCipher : IDecryptCipher
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
            var reader = new BundleBinaryReader(input);
            var magic = reader.ReadStringZeroTerm();
            if (magic != FileStreamBundleHeader.UnityFSMagic)
            {
                input.Seek(0, SeekOrigin.Begin);
                input.CopyTo(output);
                return;
            }

            var version = reader.ReadUInt32();
            var unityVer = reader.ReadStringZeroTerm();
            var unityRev = reader.ReadStringZeroTerm();

            var totalSize = reader.ReadInt64();
            var compressedBlocksInfoSize = reader.ReadUInt32();
            var uncompressedBlocksInfoSize = reader.ReadUInt32();
            var flags = (BundleFlags)reader.ReadUInt32();
            if (version >= 7)
            {
                reader.AlignStream(16);
            }

            var compressionType = (UnityCompressionType)(flags & BundleFlags.CompressionTypeMask);
            if (compressionType != UnityCompressionType.None && compressionType != UnityCompressionType.Lz4HC)
            {
                // Not supported
                input.Seek(0, SeekOrigin.Begin);
                input.CopyTo(output);
                return;
            }

            var blocksInfo = reader.ReadBytes((int)compressedBlocksInfoSize);

            Stream blocksInfoStream;
            if (compressionType == UnityCompressionType.Lz4HC)
            {
                var uncompressedBytes = new byte[uncompressedBlocksInfoSize];
                var numWrite = LZ4Codec.Decode(blocksInfo, uncompressedBytes);
                if (numWrite != uncompressedBlocksInfoSize)
                {
                    throw new IOException($"Lz4 decompression error, write {numWrite} bytes but expected {uncompressedBlocksInfoSize} bytes");
                }
                blocksInfoStream = new MemoryStream(uncompressedBytes);
            }
            else
            {
                blocksInfoStream = new MemoryStream(blocksInfo);
            }

            using var blockInfoReader = new BundleBinaryReader(blocksInfoStream);
            blockInfoReader.ReadBytes(16);
            var blockCount = blockInfoReader.ReadInt32();
            var uncompressedBlockSize = blockInfoReader.ReadUInt32();
            var compressedBlockSize = blockInfoReader.ReadUInt32();

            if ((flags & BundleFlags.BlockInfoNeedPaddingAtStart) != 0)
            {
                reader.AlignStream(16);
            }

            var encBlockSize = compressedBlockSize < 0x500 ? (int)compressedBlockSize : 0x500;
            var encPos = reader.Position;
            input.Seek(0, SeekOrigin.Begin);
            input.CopyTo(output, encPos);
            var buffer = ArrayPool<byte>.Shared.Rent(encBlockSize);
            try
            {
                input.ReadExactly(buffer, 0, encBlockSize);
                Decrypt(buffer.AsSpan()[..encBlockSize]);
                output.Write(buffer, 0, encBlockSize);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
            input.CopyTo(output);

        }

        public static void Decrypt(Span<byte> encData)
        {
            var encLength = encData.Length;

            var encDataInt = encData.As<uint>();

            var encBlock1 = (stackalloc uint[4]);
            encBlock1[0] = encDataInt[2] ^ encDataInt[5] ^ 0x3F72EAF3u;
            encBlock1[1] = encDataInt[3] ^ encDataInt[7] ^ (uint)encLength;
            encBlock1[2] = encDataInt[1] ^ encDataInt[4] ^ (uint)encLength ^ 0x753BDCAAu;
            encBlock1[3] = encDataInt[0] ^ encDataInt[6] ^ 0xE3D947D3u;

            // Surprise tool for later :)
            var encBlock2Key = (stackalloc byte[4]);
            GenerateKey(ref encBlock1, out encBlock2Key);
            var encBlock2KeyInt = encBlock2Key.As<uint>()[0];

            var encBlock1Key = (uint)encLength ^ encBlock1[0] ^ encBlock1[1] ^ encBlock1[2] ^ encBlock1[3] ^ 0x5E8BC918u;

            var encBlockRc4 = new Rc4(kb => (byte)(kb.RotateLeft(1) - 0x61));
            encBlockRc4.Decrypt(encBlock1.AsBytes(), BitConverter.GetBytes(encBlock1Key));

            var crc = Crc32.Hash(encBlock1.AsBytes());

            for (int i = 0; i < 32; i++)
            {
                encData[i] ^= 0xb7;
            }

            if (encLength == 32)
            {
                return;
            }

            if (encLength < 0x9f)
            {
                encBlockRc4.Decrypt(encData[32..], encBlock2Key);
                return;
            }

            var keyMaterial2 = (stackalloc uint[4]);
            keyMaterial2[0] = (encBlock1[3] + 0x6F1A36D8u) ^ (crc + 0x2);
            keyMaterial2[1] = (encBlock1[2] - 0x7E9A2C76u) ^ encBlock2KeyInt;
            keyMaterial2[2] = encBlock1[0] ^ 0x840CF7D0u ^ (crc + 0x2);
            keyMaterial2[3] = (encBlock1[1] + 0x48D0E844) ^ encBlock2KeyInt;

            var keyBlockKey = (stackalloc byte[4]);
            GenerateKey(ref keyMaterial2, out keyBlockKey);

            var encBlock2 = encData.Slice(0x20, 0x80);
            var keyBlock = encBlock2.ToArray().AsSpan();
            var keyBlockInt = keyBlock.As<uint>();

            encBlockRc4.Decrypt(keyBlock, keyBlockKey);
            encBlockRc4.Decrypt(encBlock2, keyMaterial2.AsBytes()[..12]);

            var keyTable2 = (stackalloc uint[9]);
            keyTable2[0] = 0x88558046u;
            keyTable2[1] = keyMaterial2[3];
            keyTable2[2] = 0x5C7782C2u;
            keyTable2[3] = 0x38922E17u;
            keyTable2[4] = keyMaterial2[0];
            keyTable2[5] = keyMaterial2[1];
            keyTable2[6] = 0x44B38670u;
            keyTable2[7] = keyMaterial2[2];
            keyTable2[8] = 0x6B07A514u;

            var encBlock3 = encData[0xa0..];
            var remainingEncSection = encLength - 0xa0;
            var remainingNonAligned = encLength - (remainingEncSection & 0xffffff80) - 0xa0;
            if (encLength >= 0x120)
            {
                const int blockSize = 0x20;
                for (int i = 0; i < remainingEncSection / 0x80; i++)
                {
                    var currentBlockSlice = encBlock3.Slice(i * blockSize * 0x4, blockSize * 0x4).As<uint>();
                    var type = keyTable2[i % 9] & 3;

                    for (int idx = 0; idx < blockSize; idx++)
                    {
                        var keyBlockVal = keyBlockInt[idx];
                        var val = type switch
                        {
                            0 => keyBlockVal ^ keyTable2[(int)(keyMaterial2[idx & 3] % 9)] ^ (uint)(blockSize - idx),
                            1 => keyBlockVal ^ keyMaterial2[(int)(keyBlockVal & 3)] ^ keyTable2[(int)(keyBlockVal % 9)],
                            2 => keyBlockVal ^ keyMaterial2[(int)(keyBlockVal & 3)] ^ (uint)idx,
                            3 => keyBlockVal ^ keyMaterial2[(int)(keyTable2[idx % 9] & 3)] ^ (uint)(blockSize - idx),
                            _ => throw new UnreachableException()
                        };

                        currentBlockSlice[idx] ^= val;
                    }
                }
            }

            if (remainingNonAligned > 0)
            {
                var totalRemainingOffset = remainingEncSection - remainingNonAligned;
                for (int i = 0; i < remainingNonAligned; i++)
                {
                    encBlock3[(int)totalRemainingOffset + i] ^= (byte)(i ^ keyBlock[i & 0x7f] ^ (byte)(keyTable2[(int)(keyMaterial2[i & 3] % 9)] % 0xff));
                }
            }
        }

        private static void GenerateKey(ref Span<uint> keyMaterial, out Span<byte> outKey)
        {
            var keyMaterialBytes = keyMaterial.AsBytes();

            var temp1 = 0x78DA0550u;
            var temp2 = 0x2947E56Bu;
            var key = 0xc1646153u;

            foreach (var byt in keyMaterialBytes)
            {
                key = 0x21 * key + byt;

                if ((key & 0xf) > 0xA)
                {
                    var xor = 1u;
                    if (temp2 >> 6 == 0)
                    {
                        xor = temp2 << 26 != 0 ? 1u : 0u;
                    }
                    key = (key ^ xor) - 0x2CD86315;
                }
                else if ((byte)key >> 4 == 0xf)
                {
                    var xor = 1u;
                    if (temp2 >> 9 == 0)
                    {
                        xor = temp2 << 23 != 0 ? 1u : 0u;
                    }
                    key = (key ^ xor) + (temp1 ^ 0xAB4A010B);
                }
                else if (((key >> 8) & 0xf) <= 1)
                {
                    temp1 = key ^ ((temp2 >> 3) - 0x55eeab7b);
                }
                else if (temp1 + 0x567A > 0xAB5489E3)
                {
                    temp1 = key ^ ((temp1 & 0xffff0000) >> 16);
                }
                else if ((temp1 ^ 0x738766FA) <= temp2)
                {
                    temp1 = temp2 ^ (temp1 >> 8);
                }
                else if (temp1 == 0x68F53AA6)
                {
                    if (((key + temp2) ^ 0x68F53AA6) > 0x594AF86E)
                    {
                        temp1 = 0x602B1178;
                    }
                    else
                    {
                        temp2 -= 0x760A1649;
                    }
                }
                else
                {
                    if (key <= 0x865703AF)
                    {
                        temp1 = key ^ (temp1 - 0x12B9DD92);
                    }
                    else
                    {
                        temp1 = (key - 0x564389D7) ^ temp2;
                    }

                    var xor = 1u;
                    if (temp1 >> 8 == 0)
                    {
                        xor = temp1 << 24 != 0 ? 1u : 0u;
                    }
                    key ^= xor;
                }
            }

            outKey = BitConverter.GetBytes(key).AsSpan();
        }

        private class Rc4(Func<byte, byte> transform)
        {
            public void Decrypt(Span<byte> data, Span<byte> key)
            {
                if (data.Length <= 0)
                {
                    return;
                }

                var kt = new byte[256];
                for (int i = 0; i < 256; i++)
                {
                    kt[i] = (byte)i;
                }

                var swap = 0;
                for (int i = 0; i < 256; i++)
                {
                    var a = kt[i];
                    swap = (swap + a + key[i % key.Length]) & 0xff;
                    kt[i] = kt[swap];
                    kt[swap] = a;
                }

                byte j = 0, k = 0;
                for (int i = 0; i < data.Length; i++)
                {
                    j++;
                    var a = kt[j];
                    k = (byte)(a + k);
                    kt[j] = kt[k];
                    kt[k] = a;

                    var kb = kt[(byte)(a + kt[j])];
                    data[i] ^= transform(kb);
                }
            }
        }

        private static class Crc32
        {
            private static readonly uint[] Lookup = new uint[256];
            static Crc32()
            {
                for (uint i = 0; i < 256; i++)
                {
                    var val = i;
                    for (uint j = 0; j < 8; j++)
                    {
                        if ((val & 1) == 0)
                        {
                            val >>= 1;
                        }
                        else
                        {
                            val = (val >> 1) ^ 0xD35E417E;
                        }
                    }
                    Lookup[i] = val;
                }
            }

            public static uint Hash(ReadOnlySpan<byte> data)
            {
                var crc = 0xffffffffu;
                foreach (var byt in data)
                {
                    crc = (Lookup[unchecked((byte)crc ^ byt)] ^ (crc >> 9)) + 0x5b;
                }

                return ~crc + 0xBE9F85C1;
            }
        }
    }
}
