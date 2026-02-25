using K4os.Compression.LZ4;
using SkiaSharp;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using ZoDream.Shared;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;

namespace ZoDream.BundleExtractor.Eastward
{
    public class HmgReader(BinaryReader reader, string fileName) : IArchiveReader
    {
        private const string MagicHeader = "PGF";

        public void ExtractTo(Stream output)
        {
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            Expectation.ThrowIfNotSignature(MagicHeader, Encoding.ASCII.GetString(reader.ReadBytes(3)));
            var length = reader.ReadByte();
            var fileSize = reader.ReadInt32();
            reader.ReadBytes(length);
            var compressedSize = reader.ReadInt32();
            var width = reader.ReadInt32();
            var height = reader.ReadInt32();
            reader.ReadByte();
            reader.ReadByte();
            length = reader.ReadByte();
            reader.ReadByte();
            reader.ReadBytes(length);
            var uncompressedSize = width * height * 4;
            var buffer = ArrayPool<byte>.Shared.Rent(compressedSize);
            var target = ArrayPool<byte>.Shared.Rent(uncompressedSize);
            try
            {
                reader.Read(buffer, 0, compressedSize);
                LZ4Codec.Decode(buffer, 0, compressedSize, target, 0, uncompressedSize);
                var newInfo = new SKImageInfo(width, height, SKColorType.Rgba8888);
                var data = SKData.CreateCopy(target.AsSpan(0, uncompressedSize));
                using var image = SKImage.FromPixels(newInfo, data);
                image.Encode(output, SKEncodedImageFormat.Png, 100);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
                ArrayPool<byte>.Shared.Return(target);
            }
        }

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            ExtractTo(output);
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            var outputFile = Path.Combine(folder, fileName);
            if (!LocationStorage.TryCreate(outputFile, ".png", mode, out outputFile))
            {
                return;
            }
            using var output = File.Create(outputFile);
            ExtractTo(new ReadOnlyEntry(fileName), output);
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            return [new ReadOnlyEntry(fileName)];
        }

        public void Dispose()
        {
            reader.Dispose();
        }

        internal static bool IsSupport(ReadOnlySpan<byte> buffer)
        {
            return buffer[0..MagicHeader.Length].SequenceEqual(Encoding.ASCII.GetBytes(MagicHeader));
        }
    }
}
