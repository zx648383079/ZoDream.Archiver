using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Threading;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.WallpaperExtractor.Models;

namespace ZoDream.WallpaperExtractor
{
    public class TexReader(BinaryReader reader, string fileName, IArchiveOptions? options) : IArchiveReader
    {
        public void Dispose()
        {
            if (options?.LeaveStreamOpen == false)
            {
                reader.Dispose();
            }
        }


        private Stream TryOpenWrite(TexHeader header, string outputFile)
        {
            var extension = header.IsGift ? "gif" : header.ImageFormat.GetFileExtension(); ;
            outputFile = PackageExtension.CombineExtension(outputFile, extension);
            PackageExtension.CreateDirectory(outputFile);
            return File.Create(outputFile);
        }

        private void ExtractToImage(TexMipmap tex, TexHeader header, 
            Func<TexHeader, Stream> cb)
        {
            using var bitmap = tex.Decode(header.Format).ToImage();
            if (bitmap == null)
            {
                return;
            }
            using var fs = cb(header);
            bitmap.Encode(fs, header.ImageFormat.Parse(), 100);
        }

        public void ExtractTo(string outputFile)
        {
            ExtractTo((header) => TryOpenWrite(header, outputFile));
        }

        internal void ExtractTo(Func<TexHeader, Stream> cb)
        {
            var (header, imageCount) = ReadHeader();
            var imageItems = new TexMipmap[imageCount][];
            for (var i = 0; i < imageCount; i++)
            {
                imageItems[i] = ReadTexImage(header.BVersion > 1);
            }
            if (!header.IsGift)
            {
                ExtractToImage(imageItems[0][0], header, cb);
                return;
            }
            var magic = reader.ReadNZeroString(16);
            Debug.Assert(magic is "TEXS0001" or "TEXS0002" or "TEXS0003");
            var frameCount = reader.ReadInt32();
            var giftWidth = 0;
            var giftHeight = 0;
            if (magic == "TEXS0003")
            {
                giftWidth = reader.ReadInt32();
                giftHeight = reader.ReadInt32();
            }
            var frameItems = new TexFrame[frameCount];
            for (var i = 0; i < frameCount; i++)
            {
                frameItems[i] = ReadTexFrame(magic != "TEXS0001");
            }
            using var fs = cb(header);
            using var gif = new GifEncoder(fs, giftWidth, giftHeight);
            foreach (var item in frameItems)
            {
                var tex = imageItems[item.ImageId];
                var bitmap = tex[0].Decode(header.Format).ToImage();
                if (bitmap is null)
                {
                    continue;
                }
                var width = item.Width != 0 ? item.Width : item.HeightX;
                var height = item.Height != 0 ? item.Height : item.WidthY;
                var x = Math.Min(item.X, item.X + width);
                var y = Math.Min(item.Y, item.Y + height);
                var rotationAngle = -(Math.Atan2(Math.Sign(height), Math.Sign(width)) - Math.PI / 4);
                gif.AddFrame(bitmap.Clip(
                    SKRectI.Create((int)x, (int)y, (int)width, (int)height)
                    )?.Rotate((float)(rotationAngle * 180 / Math.PI)),
                    (int)Math.Round(item.FrameTime * 100));
            }
        }

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            ExtractTo(_ => output);
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            ExtractTo(Path.Combine(folder, fileName));
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            var (header, _) = ReadHeader();
            var extension = header.IsGift ? "gif" : header.ImageFormat.GetFileExtension();
            return [new ReadOnlyEntry(
                PackageExtension.CombineExtension(fileName, extension)
             , reader.BaseStream.Length)
            ];
        }


        private (TexHeader, int) ReadHeader()
        {
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            var magic = reader.ReadNZeroString(16);
            Debug.Assert(magic == "TEXV0005");
            magic = reader.ReadNZeroString(16);
            Debug.Assert(magic == "TEXI0001");
            var header = new TexHeader()
            {
                Format = (TexFormat)reader.ReadInt32(),
                Flags = (TexFlags)reader.ReadInt32(),
                TextureWidth = reader.ReadInt32(),
                TextureHeight = reader.ReadInt32(),
                ImageWidth = reader.ReadInt32(),
                ImageHeight = reader.ReadInt32(),
                UnkInt0 = reader.ReadUInt32()
            };
            magic = reader.ReadNZeroString(16);
            Debug.Assert(magic is "TEXB0001" or "TEXB0002" or "TEXB0003");
            header.BVersion = int.Parse(magic[4..]);
            var imageCount = reader.ReadInt32();
            header.ImageFormat = header.BVersion == 3 ? (FreeImageFormat)reader.ReadInt32() : FreeImageFormat.FIF_UNKNOWN;
            return (header, imageCount);
        }

        private TexMipmap[] ReadTexImage(bool versionV2Laster)
        {
            var mipmapCount = reader.ReadInt32();
            var items = new TexMipmap[mipmapCount];
            for (var i = 0; i < mipmapCount; i++)
            {
                items[i] = ReadTexMipmap(versionV2Laster);
            }
            return items;
        }

        private TexMipmap ReadTexMipmap(bool versionV2Laster)
        {
            var width = reader.ReadInt32();
            var height = reader.ReadInt32();
            var isLZ4Compressed = false;
            var decompressedBytesCount = 0;
            if (versionV2Laster)
            {
                isLZ4Compressed = reader.ReadInt32() == 1;
                decompressedBytesCount = reader.ReadInt32();
            }
            var length = reader.ReadInt32();
            return new TexMipmap(width, height, decompressedBytesCount,
                isLZ4Compressed, new PartialStream(reader.BaseStream, length));
        }

        private TexFrame ReadTexFrame(bool versionV2Laster)
        {
            if (!versionV2Laster)
            {
                return new TexFrame()
                {
                    ImageId = reader.ReadInt32(),
                    FrameTime = reader.ReadSingle(),
                    X = reader.ReadInt32(),
                    Y = reader.ReadInt32(),
                    Width = reader.ReadInt32(),
                    WidthY = reader.ReadInt32(),
                    HeightX = reader.ReadInt32(),
                    Height = reader.ReadInt32(),
                };
            }
            return new TexFrame()
            {
                ImageId = reader.ReadInt32(),
                FrameTime = reader.ReadSingle(),
                X = reader.ReadSingle(),
                Y = reader.ReadSingle(),
                Width = reader.ReadSingle(),
                WidthY = reader.ReadSingle(),
                HeightX = reader.ReadSingle(),
                Height = reader.ReadSingle(),
            };
        }

    }
}
