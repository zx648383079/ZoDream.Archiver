using System;
using System.Buffers.Binary;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using ZoDream.BundleExtractor.Unity.BundleFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public partial class OtherBundleElementScanner
    {
        internal static void JumpNotZeroString(Stream input)
        {
            while (input.ReadByte() > 0)
            {
            }
        }

        internal static long GetBundleFileSize(Stream input, long pos)
        {
            input.Position = pos;
            JumpNotZeroString(input);// UnityFS
            input.Seek(4, SeekOrigin.Current); // version
            JumpNotZeroString(input); // unityVersion
            JumpNotZeroString(input); // unityRevision
            return BinaryPrimitives.ReadInt64BigEndian(input.ReadBytes(8));
        }

        private Stream ParseXor(Stream input)
        {
            if (!package.TryGet<XorCommandArgument>("xor", out var args))
            {
                return input;
            }
            return ParseXor(input, args);
        }

        public static Stream ParseXor(Stream input, XorCommandArgument args)
        {
            if (input.Length < FileStreamBundleHeader.UnityFSMagic.Length)
            {
                return input;
            }
            var beginAt = input.Position;
            var next = new XORStream(input, args.Keys, args.MaxPosition);
            var magic = next.ReadBytes(FileStreamBundleHeader.UnityFSMagic.Length);
            input.Position = beginAt;
            if (magic.Equal(FileStreamBundleHeader.UnityFSMagic))
            {
                return next;
            }
            return input;
        }
        public static long FindUnityFS(Stream input)
        {
            var finder = new StreamFinder(FileStreamBundleHeader.UnityFSMagic)
            {
                IsMatchFirst = true,
                MaxPosition = 1024
            };
            while (finder.MatchFile(input))
            {
                var pos = finder.BeginPosition.First();
                var size = GetBundleFileSize(input, pos);
                if (pos + size == input.Length)
                {
                    return pos;
                }
            }
            input.Position = 0;
            return -1;
        }
        public static Stream ParseFakeHeader(Stream input)
        {
            var finder = new StreamFinder(FileStreamBundleHeader.UnityFSMagic)
            {
                IsMatchFirst = true,
                MaxPosition = 1024
            };
            while (finder.MatchFile(input))
            {
                var pos = finder.BeginPosition.First();
                var size = GetBundleFileSize(input, pos);
                if (pos + size == input.Length)
                {
                    return pos == 0 ? input : new PartialStream(input, pos, size);
                }
            }
            input.Position = 0;
            return input;
        }


        private Stream DecryptAcmeis(Stream input)
        {
            return ParseFakeHeader(input);
        }

        private Stream DecryptAnchorPanic(Stream input, string fullPath)
        {
            var finder = new StreamFinder(FileStreamBundleHeader.UnityFSMagic)
            {
                IsMatchFirst = true,
                MaxPosition = 1024
            };
            if (finder.MatchFile(input))
            {
                input.Position = finder.BeginPosition.First();
                return ParseFakeHeader(input);
            }
            return new AnchorPanicStream(input, fullPath);
        }

        private IBundleBinaryReader DecryptProjectSekai(Stream input)
        {
            var version = BitConverter.ToUInt32(input.ReadBytes(4));
            if (version != 0x10 && version != 0x20)
            {
                input.Position = 0;
                return new BundleBinaryReader(input, EndianType.BigEndian);
            }
            if (version != 0x10)
            {
                return new BundleBinaryReader(
                    new PartialStream(input, input.Length - input.Position), 
                    EndianType.LittleEndian);
            }
            return new BundleBinaryReader(
                    new ProjectSekaiStream(input),
                    EndianType.LittleEndian);
        }

        private Stream DecryptCodenameJump(Stream input)
        {
            var output = new CodenameJumpStream(input);
            var signature = output.ReadBytes(7);
            input.Position = 0;
            if (Encoding.ASCII.GetString(signature) == FileStreamBundleHeader.UnityFSMagic)
            {
                return output;
            }
            return input;
        }
        private Stream DecryptGirlsFrontline(Stream input)
        {
            return new GirlsFrontlineStream(input);
        }
        private Stream DecryptJJKPhantomParade(Stream input)
        {
            return new JJKPhantomParadeStream(input);
        }
        private Stream DecryptMuvLuvDimensions(Stream input)
        {
            return new MuvLuvDimensionsStream(input);
        }
        private Stream DecryptPartyAnimals(Stream input, string fullPath)
        {
            return new PartyAnimalsStream(input, fullPath);
        }
        private Stream DecryptSchoolGirlStrikers(Stream input)
        {
            return new SchoolGirlStrikersStream(input);
        }

        private Stream DecryptCounterSide(Stream input, string fullPath)
        {
            var buffer = new byte[input.Length];
            input.ReadExactly(buffer);

            var decryptSize = Math.Min(buffer.Length, 212);
            var fileName = Path.GetFileNameWithoutExtension(fullPath);
            var hash = MD5.HashData(Encoding.UTF8.GetBytes(fileName.ToLower()));
            var hex = Convert.ToHexString(hash);
            ulong[] MaskList = [0UL, 0UL, 0UL, 0UL];
            MaskList[0] = ulong.Parse(hex[..16], System.Globalization.NumberStyles.HexNumber);
            MaskList[1] = ulong.Parse(hex.Substring(16, 16), System.Globalization.NumberStyles.HexNumber);
            MaskList[2] = ulong.Parse(string.Concat(hex.AsSpan(0, 8), hex.AsSpan(16, 8)), System.Globalization.NumberStyles.HexNumber);
            MaskList[3] = ulong.Parse(string.Concat(hex.AsSpan(8, 8), hex.AsSpan(24, 8)), System.Globalization.NumberStyles.HexNumber);
            var pos = 0;
            var maskPos = 0;
            while (pos < decryptSize)
            {
                if (decryptSize - pos > 7)
                {
                    var value = BitConverter.ToUInt64(buffer, pos);
                    value ^= MaskList[maskPos];
                    Buffer.BlockCopy(BitConverter.GetBytes(value), 0, buffer, pos, 8);
                    pos += 8;
                }
                else
                {
                    var p = 0;
                    while (pos + p < decryptSize)
                    {
                        buffer[pos + p] ^= (byte)((0xFFFFFFFFFFFFFFFF >> p) & MaskList[maskPos]);
                        p += 1;
                    }
                    pos = decryptSize;
                }
                maskPos = (maskPos + 1) % 4;
            }

            MemoryStream ms = new();
            ms.Write(buffer);
            ms.Position = 0;
            input.Dispose();
            return ms;
        }
    }
}
