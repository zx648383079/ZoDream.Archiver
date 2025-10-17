using System;
using System.Buffers;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using ZoDream.BundleExtractor.Compression;
using ZoDream.BundleExtractor.Unity.Scanners;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Logging;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.RpgMarker
{
    public partial class MvScheme(IBundleChunk fileItems, IEntryService service, IBundleOptions options) : IBundleHandler
    {
        const int HeaderLength = 16;
        private static readonly byte[] Signature = "RPGMV"u8.ToArray();
        private static readonly byte[] PngHeaderBytes = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00, 0x00, 0x00, 0x0D, 0x49, 0x48, 0x44, 0x52];

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            var logger = service.Get<ILogger>();
            if (string.IsNullOrEmpty(options.Password))
            {
                logger.Error("Key is empty");
                return;
            }
            var buffer = new byte[16];
            var keys = Convert.FromHexString(options.Password);
            var progress = logger.CreateSubProgress("Decrypt ...", fileItems.Count);
            foreach (var item in fileItems.Items)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                using var fs = fileItems.OpenRead(item);
                if (fs.Length <  HeaderLength * 2)
                {
                    progress?.Add(1);
                    continue;
                }
                fs.ReadExactly(buffer);
                if (!buffer.StartsWith(Signature))
                {
                    progress?.Add(1);
                    continue;
                }
                var i = item.Name.LastIndexOf('.');
                var fileName = fileItems.Create(item, item.Name[..i] + GetExtension(item.Name[i..]), folder);
                if (!LocationStorage.TryCreate(fileName, mode, out fileName))
                {
                    progress?.Add(1);
                    continue;
                }
                new XORStream(new PartialStream(fs), keys, keys.Length - 1).SaveAs(fileName);
                progress?.Add(1);
            }
        }

        public void Dispose()
        {
        }

        private static string GetExtension(string extension)
        {
            if (extension.EndsWith('_'))
            {
                return extension.TrimEnd('_');
            }
            return extension.ToLower() switch
            {
                ".rpgmvp" => ".png",
                ".rpgmvm" => ".m4a",
                ".rpgmvo" => ".ogg"
            };
        }


        public static bool TryGetKeyFromJson(Stream input, out string key)
        {
            key = string.Empty;
            var text = LocationStorage.ReadText(input);
            if (!text.Contains('{'))
            {
                text = LZString.DecompressFromBase64(text);
            }
            var match = JsonKeyRegex().Match(text);
            if (match.Success)
            {
                key = match.Groups[1].Value;
                return true;
            }
            return false;
        }

        public static string GetKey(Stream input)
        {
            var length = PngHeaderBytes.Length;
            var buffer = ArrayPool<byte>.Shared.Rent(length);
            try
            {
                var key = buffer.AsSpan(0, length);
                return TryGetKey(input, key) ? Convert.ToHexString(key) : string.Empty;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        private static bool TryGetKey(Stream input, Span<byte> keys)
        {
            input.Position = 0;
            var buffer = ArrayPool<byte>.Shared.Rent(HeaderLength);
            try
            {
                if (buffer.Length < HeaderLength * 2)
                {
                    return false;
                }
                input.ReadExactly(buffer, 0, HeaderLength);
                if (!buffer.StartsWith(Signature))
                {
                    return false;
                }
                input.ReadExactly(buffer, 0, HeaderLength);
                for (var i = 0; i < PngHeaderBytes.Length; i++)
                {
                    keys[i] = (byte)(buffer[i] ^ PngHeaderBytes[i]);
                }
                return true;
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }
        }

        [GeneratedRegex(@"""encryptionKey""\s*:\s*""([^""]+)""")]
        private static partial Regex JsonKeyRegex();
    }
}
