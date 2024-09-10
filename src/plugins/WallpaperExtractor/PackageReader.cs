using SkiaSharp;
using System.Diagnostics;
using ZoDream.WallpaperExtractor.Models;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using System.IO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ZoDream.WallpaperExtractor
{
    public class PackageReader(BinaryReader reader, IArchiveOptions? options) : IArchiveReader
    {
        private readonly long _basePosition = reader.BaseStream.Position;
        public void Dispose()
        {
            if (options?.LeaveStreamOpen == false)
            {
                reader.Dispose();
            }
        }

        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            if (entry is not PackageEntry e)
            {
                return;
            }
            reader.BaseStream.Seek(e.Offset, SeekOrigin.Begin);
            reader.BaseStream.CopyTo(output, e.Length);
        }

        public void ExtractToDirectory(string folder, Action<double>? progressFn = null, CancellationToken token = default)
        {
            var entries = ReadEntry();
            var i = 0;
            foreach (var item in entries)
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                var fileName = Path.Combine(folder, item.Name);
                if (!item.Name.EndsWith(".tex"))
                {
                    ExtractTo(item, fileName);
                }
                else
                {
                    ReadTex(ConvertTo((PackageEntry)item), fileName);
                }
                progressFn?.Invoke((double)(++i) / entries.Count());
            }
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            reader.BaseStream.Seek(_basePosition, SeekOrigin.Begin);
            reader.ReadNString();
            var count = reader.ReadInt32();
            var entries = new PackageEntry[count];
            for (var i = 0; i < count; i++)
            {
                entries[i] = new PackageEntry(
                    reader.ReadNString(),
                    reader.ReadInt32(),
                    reader.ReadInt32()
                    );
            }
            var begin = reader.BaseStream.Position;
            foreach (var item in entries)
            {
                item.Offset += begin;
            }
            return entries;
        }

        private void ExtractTo(IReadOnlyEntry entry, string fileName)
        {
            PackageExtension.CreateDirectory(fileName);
            using var fs = File.Create(fileName);
            ExtractTo(entry, fs);
        }

        private PartialStream ConvertTo(PackageEntry entry)
        {
            return new PartialStream(reader.BaseStream, entry.Offset, entry.Length);
        }

        private void ReadTex(Stream stream, string outputFile)
        {
            var r = new BinaryReader(stream);
            var magic = r.ReadNZeroString(16);
            Debug.Assert(magic == "TEXV0005");
            magic = r.ReadNZeroString(16);
            Debug.Assert(magic == "TEXI0001");
            var header = new TexHeader()
            {
                Format = (TexFormat)r.ReadInt32(),
                Flags = (TexFlags)r.ReadInt32(),
                TextureWidth = r.ReadInt32(),
                TextureHeight = r.ReadInt32(),
                ImageWidth = r.ReadInt32(),
                ImageHeight = r.ReadInt32(),
                UnkInt0 = r.ReadUInt32()
            };
            magic = r.ReadNZeroString(16);
            Debug.Assert(magic is "TEXB0001" or "TEXB0002" or "TEXB0003");
            var imageCount = r.ReadInt32();
            var imageFormat = magic == "TEXB0003" ? (FreeImageFormat)r.ReadInt32() : FreeImageFormat.FIF_UNKNOWN;
            var imageItems = new TexMipmap[imageCount][];
            for (var i = 0; i < imageCount; i++)
            {
                imageItems[i] = ReadTexImage(r, magic != "TEXB0001");
            }
            if (!header.IsGift)
            {
                imageItems[0][0].Decode(header.Format, imageFormat, outputFile);
                return;
            }
            magic = r.ReadNZeroString(16);
            Debug.Assert(magic is "TEXS0001" or "TEXS0002" or "TEXS0003");
            var frameCount = r.ReadInt32();
            if (magic == "TEXS0003")
            {
                var giftWidth = r.ReadInt32();
                var giftHeight = r.ReadInt32();
            }
            var frameItems = new TexFrame[frameCount];
            for (var i = 0; i < frameCount; i++)
            {
                frameItems[i] = ReadTexFrame(r, magic != "TEXS0001");
            }
            var fileName = PackageExtension.CombineExtension(
                outputFile, "gif");
            PackageExtension.CreateDirectory(fileName);
            using var fs = File.Create(fileName);
            using var gif = new GifEncoder(fs);
            foreach (var item in frameItems)
            {
                var tex = imageItems[item.ImageId];
                var bitmap = tex[0].Decode(header.Format).TryParse();
                if (bitmap is null)
                {
                    continue;
                }
                var width = item.Width != 0 ? item.Width : item.HeightX;
                var height = item.Height != 0 ? item.Height : item.WidthY;
                var x = Math.Min(item.X, item.X + width);
                var y = Math.Min(item.Y, item.Y + height);
                var rotationAngle = -(Math.Atan2(Math.Sign(height), Math.Sign(width)) - Math.PI / 4);
                var res = bitmap.Clip(
                    SKRect.Create(x, y, width, height)
                    );
                gif.AddFrame(res.Rotate((float)(rotationAngle * 180 / Math.PI)), 
                    (int)Math.Round(item.FrameTime * 100));
            }
        }

        private TexMipmap[] ReadTexImage(BinaryReader r, bool versionV2Laster)
        {
            var mipmapCount = r.ReadInt32();
            var items = new TexMipmap[mipmapCount];
            for (var i = 0; i < mipmapCount; i++)
            {
                items[i] = ReadTexMipmap(r, versionV2Laster);
            }
            return items;
        }

        private TexMipmap ReadTexMipmap(BinaryReader r, bool versionV2Laster)
        {
            var width = r.ReadInt32();
            var height = r.ReadInt32();
            var isLZ4Compressed = false;
            var decompressedBytesCount = 0;
            if (versionV2Laster)
            {
                isLZ4Compressed = r.ReadInt32() == 1;
                decompressedBytesCount = r.ReadInt32();
            }
            var length = r.ReadInt32();
            return new TexMipmap(width, height, decompressedBytesCount,
                isLZ4Compressed, new PartialStream(r.BaseStream, length));
        }

        private TexFrame ReadTexFrame(BinaryReader r, bool versionV2Laster)
        {
            if (!versionV2Laster)
            {
                return new TexFrame()
                {
                    ImageId = r.ReadInt32(),
                    FrameTime = r.ReadSingle(),
                    X = r.ReadInt32(),
                    Y = r.ReadInt32(),
                    Width = r.ReadInt32(),
                    WidthY = r.ReadInt32(),
                    HeightX = r.ReadInt32(),
                    Height = r.ReadInt32(),
                };
            }
            return new TexFrame()
            {
                ImageId = r.ReadInt32(),
                FrameTime = r.ReadSingle(),
                X = r.ReadSingle(),
                Y = r.ReadSingle(),
                Width = r.ReadSingle(),
                WidthY = r.ReadSingle(),
                HeightX = r.ReadSingle(),
                Height = r.ReadSingle(),
            };
        }


    }
}
