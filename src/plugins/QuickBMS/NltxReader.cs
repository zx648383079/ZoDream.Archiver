using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using ZoDream.Shared;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;
using NotSupportedException = ZoDream.Shared.NotSupportedException;

namespace ZoDream.QuickBMS
{
    public class NltxReader(IBundleBinaryReader reader, string fileName, IArchiveOptions options) : IArchiveReader, IBundleHandler
    {
        internal static readonly string Signature = "NMPLTEX1";

        public void ExtractTo(string folder,
            ArchiveExtractMode mode,
            CancellationToken token = default)
        {
            if (!LocationStorage.TryCreate(Path.Combine(folder, fileName), ".png",  mode, out var fullPath))
            {
                return;
            }
            using var fs = File.Create(fullPath);
            ExtractTo(fs);
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            reader.Position = 0;
            var buffer = reader.ReadBytes(Signature.Length);
            Expectation.ThrowIfNotSignature(buffer.Equal(Signature));
            yield return new ReadOnlyEntry(fileName);
        }

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            try
            {
                ExtractTo(output);
            }
            catch (Exception ex)
            {
            }
        }

        private void ExtractTo(Stream output)
        {
            reader.Position = Signature.Length;
            var align = reader.ReadInt64();
            var unknown0x10 = reader.ReadInt32();
            var pFormat = reader.ReadInt16();
            var unknown0x16 = reader.ReadInt16();
            var width = reader.ReadInt32();
            var height = reader.ReadInt32();
            var unknown0x20 = reader.ReadByte();
            var unknown0x21 = reader.ReadByte();
            var unknown0x22 = reader.ReadInt16();
            var unknown0x24 = reader.ReadInt32();
            var unknown0x28 = reader.ReadInt32();
            var uncompressedSize = reader.ReadInt32();


            reader.Position += 0x50;
            var decompressed = YKCMP.Decompress(reader.BaseStream.Take(reader.RemainingLength));

            switch (pFormat)
            {
                case 1: // 8bppIndexed
                    {
                        var palLength = decompressed.Length - uncompressedSize; //must be 1024 bytes (256-color palette)
                        var data = new byte[width * height * 4];
                        for (int y = 0; y < height; y++)
                        {
                            for (int x = 0; x < width; x++)
                            {
                                var to = (y * width + x) * 4;
                                var from = decompressed[y * width + x + palLength] * 4;
                                data[to] = decompressed[from];
                                data[to + 1] = decompressed[from + 1];
                                data[to + 2] = decompressed[from + 2];
                                data[to + 3] = decompressed[from + 3];
                            }
                        }
                        using var image = BitmapFactory.Decode(data, width, height, BitmapFormat.RGBA8888).ToImage();
                        image?.Encode(output, SKEncodedImageFormat.Png, 100);
                        break;
                    }
                case 2: // ARGB8888
                    {
                        var data = BitmapFactory.Decode(decompressed, width, height, BitmapFormat.ARGB8888);
                        using var image = data.ToImage();
                        image?.Encode(output, SKEncodedImageFormat.Png, 100);
                        break;
                    }
                default:
                    {
                        throw new NotSupportedException("Unknown pixel data.");
                    }
            }
        }

        public void ExtractToDirectory(string folder,
            ArchiveExtractMode mode,
            Action<double>? progressFn = null,
            CancellationToken token = default)
        {
            ExtractTo(folder, mode, token);
        }

        public void Dispose()
        {
            if (options?.LeaveStreamOpen == true)
            {
                return;
            }
            reader.Dispose();
        }
    }
}
