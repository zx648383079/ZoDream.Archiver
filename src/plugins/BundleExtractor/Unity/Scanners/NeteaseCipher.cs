using K4os.Compression.LZ4;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using ZoDream.BundleExtractor.Unity.BundleFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal class NeteaseCipher : IDecryptCipher
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
                // Not supported by netease
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

            var encBlockSize = compressedBlockSize < 0x1000 ? (int)compressedBlockSize : 0x1000;
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

        private static int GetObfuscatedVersionOffset(Span<byte> enc) // Checks encryption of AssetBundleVersion 
        {
            if (enc.Length < 64)
            {
                return -1;
            }

            const uint check = 0x7e07;

            for (int i = 0; i < 64; i++)
            {
                var magic = BitConverter.ToUInt16(enc.Slice(i, 2));
                if (magic != 0xddee)
                {
                    continue;
                }

                var packedUnityVerYear = BitConverter.ToUInt16(enc.Slice(i + 2, 2));
                if (packedUnityVerYear - 0x2017 > 0xe)
                {
                    continue;
                }

                var bit = packedUnityVerYear - 0x2017;
                if ((check & 1 << bit) == 0)
                {
                    continue;
                }
                return i;
            }

            return -1;
        }

        private static void Decrypt(Span<byte> enc)
        {
            var verOffset = GetObfuscatedVersionOffset(enc); // Offset after the encrypted year
            DecryptVersion(enc, verOffset);
            var encSectionOffset = (verOffset > 0x1f ? 0x10 : 0) + 0x30;
            DecryptData(enc, encSectionOffset);
        }

        private static void DecryptVersion(Span<byte> enc, int verOffset)
        {
            var packedUnityVerYear = BitConverter.ToUInt16(enc.Slice(verOffset + 2, 2));

            // This check only works for version 3 of the encryption.
            // We know the encrypted section length already, so this is skippable
            /*if (enc[verOffset + 4] == 0xaa && enc[verOffset + 4 + 2] == 0xbb)
                encSectionLength = 0x1000;
            else
            {
                encSectionLength = enc[verOffset + 4] * 0x10 + enc[verOffset + 4 + 2];
            }*/

            // Unpack unity year back to 4 bytes (21 20 -> 2021)
            enc[verOffset] = (byte)((packedUnityVerYear >> 12) & 0xf | 0x30);
            enc[verOffset + 1] = (byte)((packedUnityVerYear >> 8) & 0xf | 0x30);
            enc[verOffset + 2] = (byte)((packedUnityVerYear >> 4) & 0xf | 0x30);
            enc[verOffset + 3] = (byte)(packedUnityVerYear & 0xf | 0x30);


            // Only required for version 3, though it doesn't break v1 since there this isnt obfuscated
            enc[verOffset + 4] = 0x2e;
            enc[verOffset + 4 + 2] = 0x2e;
            if (enc[verOffset + 4 + 4] == verOffset)
                enc[verOffset + 4 + 4] = 0x66; // f
            if (enc[verOffset + 4 + 5] == verOffset)
                enc[verOffset + 4 + 5] = 0x66; // f
        }

        private static void DecryptData(Span<byte> enc, int encSectionOffset)
        {
            var actualEncryptedLength = (uint)(enc.Length - encSectionOffset);

            var crcInts = MemoryMarshal.Cast<byte, uint>(enc.Slice(encSectionOffset, 0x20)).ToArray();
            var crcBytes =
                    BitConverter.GetBytes(crcInts[3])
                    .Concat(BitConverter.GetBytes(crcInts[1]))
                    .Concat(BitConverter.GetBytes(crcInts[4]))
                    .Concat(BitConverter.GetBytes(actualEncryptedLength))
                    .Concat(BitConverter.GetBytes(crcInts[2]))
                    .ToArray();

            var crc = Crc32.Hash(crcBytes);

            // Decrypt the CRC'ed area
            for (int i = 0; i < 0x20; i++)
                enc[encSectionOffset + i] ^= 0xa6;

            // It's a surprise tool that will help us later!
            uint[] crcKey = {
            crc ^ crcInts[5] + 0x1985,
            crc ^ crcInts[7] + 0x1981,
            crc ^ actualEncryptedLength + 0x2013,
            crc ^ crcInts[6] + 0x2018
        };

            var actualEncryptedOffset = encSectionOffset + 0x20;
            if (actualEncryptedLength > 0x9f)
            {
                var keyBlock = enc.Slice(actualEncryptedOffset, 0x80).ToArray();
                var keyBlockInt = MemoryMarshal.Cast<byte, uint>(keyBlock);

                DecryptRc4(enc, actualEncryptedOffset, 0x80, BitConverter.GetBytes(crc));

                var rc4Key2 = BitConverter.GetBytes(crcKey[2]); // Not actually the array reference but the same value
                DecryptRc4(keyBlock, 0, 0x80, rc4Key2); // Because it was so fun the first time

                uint[] keyTable2 =
                {
                    0x571u,
                    crcKey[3],
                    0x892u,
                    0x750u,
                    crcKey[0],
                    crcKey[1],
                    0x746u,
                    crcKey[2],
                    0x568u
                };

                var remainingEncSection = actualEncryptedLength - 0xa0;
                var remainingNonAligned = actualEncryptedLength - (remainingEncSection & 0xffffff80) - 0xa0;
                if (actualEncryptedLength >= 0x120)
                {
                    var currentBlockOffset = actualEncryptedOffset + 0x80;
                    for (int i = 0; i < remainingEncSection / 0x80; i++)
                    {
                        var type = keyTable2[i % 9] & 3;

                        Func<uint, uint, uint> getValFunc = type switch
                        {
                            0 => (idx, keyBlockVal) => keyTable2[idx % 9] ^ keyBlockVal ^ (32u - idx),
                            1 => (idx, keyBlockVal) => crcKey[keyBlockVal & 3] ^ keyBlockVal,
                            2 => (idx, keyBlockVal) => crcKey[keyBlockVal & 3] ^ keyBlockVal ^ idx,
                            3 => (idx, keyBlockVal) => crcKey[keyTable2[idx % 9] & 3] ^ keyBlockVal ^ idx,
                            _ => throw new UnreachableException()
                        };

                        var currentBlockSpan = MemoryMarshal.Cast<byte, uint>(enc.Slice(currentBlockOffset, 0x80));

                        for (int j = 0; j < 32; j++)
                        {
                            var keyBlockVal = keyBlockInt[j];
                            var val = getValFunc((uint)j, keyBlockVal);
                            currentBlockSpan[j] ^= val;
                        }

                        currentBlockOffset += 0x80;
                    }
                }

                if (remainingNonAligned > 0)
                {
                    var totalRemainingOffset = encSectionOffset + actualEncryptedLength - remainingNonAligned;
                    for (int i = 0; i < remainingNonAligned; i++)
                    {
                        enc[(int)totalRemainingOffset + i] ^= (byte)(i ^ keyBlock[i & 0x7f] ^ (byte)(keyTable2[crcKey[i & 3] % 9] + keyTable2[crcKey[i & 3] % 9] / 0xff));
                    }
                }
            }
            else
            {
                DecryptRc4(enc, actualEncryptedOffset, (int)actualEncryptedLength - 0x20, BitConverter.GetBytes(crc));
            }
        }

        private static void DecryptRc4(Span<byte> data, int offset, int length, Span<byte> key)
        {
            var kt = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                kt[i] = (byte)i;
            }

            var swap = 0;
            for (int i = 0; i < 256; i++)
            {
                // No idea why they do it like this.. compare to normal rc4 and replace if equal
                var a = kt[i];
                var b = a + swap;
                var c = key[i & 3];
                var d = c + b + 0xff;
                var e = c + b;
                if (e >= 0)
                {
                    d = e;
                }
                swap = e - (d & 0xffff00);
                kt[i] = kt[swap];
                kt[swap] = a;
            }

            if (length > 0)
            {
                byte j = 0, k = 0;
                for (int i = 0; i < length; i++)
                {
                    j++;
                    var a = kt[j];
                    k = (byte)(a + k);
                    kt[j] = kt[k];
                    kt[k] = a;

                    uint kb = kt[(byte)(a + kt[j])];
                    var rot = (byte)((kb << 6) | (kb >> 2));
                    data[offset + i] ^= (byte)(rot + 0x3a);
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
                            val >>= 1;
                        else
                            val = (val >> 1) ^ 0x4c11eb7;
                    }

                    Lookup[i] = val;
                }
            }

            public static uint Hash(ReadOnlySpan<byte> data)
            {
                if (data.Length == 0)
                {
                    return 0x82D63B78;
                }

                var xor = 0xffffffffu;
                var crc = 0u;
                foreach (var byt in data)
                {
                    crc = Lookup[unchecked((byte)xor ^ byt)] ^ (xor >> 8);
                    xor = crc + 16;
                }

                return 0x82D63B67 - crc;
            }
        }
    }
}
