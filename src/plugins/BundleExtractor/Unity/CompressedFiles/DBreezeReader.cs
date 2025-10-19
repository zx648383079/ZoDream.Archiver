using K4os.Compression.LZ4.Streams;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.Converters;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Drawing;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;
using ZoDream.Shared.Storage;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.CompressedFiles
{
    public class DBreezeReader(Stream input) : IArchiveReader, IRawEntryReader
    {

        public const string FileName = "_DBreezeResources";

        private readonly byte[] _buffer = new byte[8];


        public void ExtractTo(IReadOnlyEntry entry, Stream output)
        {
            if (entry is not ArchiveEntry o)
            {
                return;
            }
            var fs = ReadStream(o);
            var readLen = fs.Read(_buffer, 0, 4);
            fs.Position = 0;
            var leaveOpen = true;
            if (readLen == 4 && _buffer.StartsWith([0x04,0x22,0x4D, 0x18]))
            {
                var ms = new MemoryStream();
                using var s = LZ4Frame.Decode(fs, 0, true).AsStream();
                s.CopyTo(ms);
                fs = ms;
                leaveOpen = false;
            }
            if (entry.Name.StartsWith("utex2."))
            {
                WriteTexture(output, fs);
            }
            else
            {
                fs.CopyTo(output);
            }
            if (!leaveOpen)
            {
                fs.Dispose();
            }
        }

        private static void WriteTexture(Stream output, Stream fs)
        {
            var reader = new BinaryReader(fs);
            fs.Position = 0x11D;
            var width = reader.ReadInt32();
            fs.Position = 0x133;
            var height = reader.ReadInt32();
            fs.Position = 0x160;
            var format = (TextureFormat)reader.ReadInt32();
            fs.Position = 0x1C1;
            var data = TextureExtension.Decode(new PartialStream(fs), width,
            height, format, Version.Parse("2018.4.34f1"));
            using var img = data?.ToImage();
            using var flip = img?.Flip(false);
            flip?.Encode(output, SkiaSharp.SKEncodedImageFormat.Png, 100);
        }

        public bool TryExtractTo(IReadOnlyEntry entry, string folder, ArchiveExtractMode mode)
        {
            if (entry is not ArchiveEntry o)
            {
                return false;
            }
            var fs = ReadStream(o);
            var readLen = fs.Read(_buffer, 0, 4);
            fs.Position = 0;
            var leaveOpen = true;
            if (readLen == 4 && _buffer.StartsWith([0x04, 0x22, 0x4D, 0x18]))
            {
                var ms = new MemoryStream();
                using var s = LZ4Frame.Decode(fs, 0, true).AsStream();
                s.CopyTo(ms);
                fs = ms;
                leaveOpen = false;
            }
            var fileName = Path.Combine(folder, entry.Name);
            var isRaw = true;
            if (entry.Name.StartsWith("utex2."))
            {
                if (LocationStorage.TryCreate(fileName, ".png", mode, out fileName))
                {
                    using var ouput = File.Create(fileName);
                    WriteTexture(ouput, fs);
                }
            }
            else
            {
                readLen = fs.Read(_buffer, 0, 8);
                fs.Position = 0;
                var ext = string.Empty;
                if (readLen >= 4 && _buffer.StartsWith("OggS"))
                {
                    ext = ".ogg";
                }
                else if (readLen == 8 && _buffer.StartsWith("UnityFS\x00"))
                {
                    ext = ".ab";
                    isRaw = false;
                }
                if (isRaw && LocationStorage.TryCreate(fileName, ext, mode, out fileName))
                {
                    fs.SaveAs(fileName);
                }
            }
            if (!leaveOpen)
            {
                fs.Dispose();
            }
            return isRaw;
        }

        private void ExtractTo(ArchiveEntry entry, string folder, ArchiveExtractMode mode)
        {
            var fs = ReadStream(entry);
            var readLen = fs.Read(_buffer, 0, 4);
            fs.Position = 0;
            var leaveOpen = true;
            if (readLen == 4 && _buffer.StartsWith([0x04, 0x22, 0x4D, 0x18]))
            {
                var ms = new MemoryStream();
                using var s = LZ4Frame.Decode(fs, 0, true).AsStream();
                s.CopyTo(ms);
                fs = ms;
                leaveOpen = false;
            }
            if (entry.Name.StartsWith("utex2."))
            {
                using var ouput = File.Create(Path.Combine(folder, entry.Name + ".png"));
                WriteTexture(ouput, fs);
            }
            else
            {
                readLen = fs.Read(_buffer, 0, 8);
                fs.Position = 0;
                var ext = string.Empty;
                if (readLen >= 4 && _buffer.StartsWith("OggS"))
                {
                    ext = ".ogg";
                }
                else if (readLen == 8 && _buffer.StartsWith("UnityFS\x00"))
                {
                    ext = ".ab";
                }
                fs.SaveAs(Path.Combine(folder, entry.Name + ext));
            }
            if (!leaveOpen)
            {
                fs.Dispose();
            }
        }

        public void ExtractToDirectory(string folder, ArchiveExtractMode mode, Action<double>? progressFn = null, CancellationToken token = default)
        {
            foreach (var item in ReadEntry())
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                ExtractTo((ArchiveEntry)item, folder, mode);
            }
        }

        public IEnumerable<IReadOnlyEntry> ReadEntry()
        {
            input.Seek(12, SeekOrigin.Begin);
            var count = ReadBeInt32(3);
            var pos = 64L;
            for (var i = 0; i < count; i++)
            {
                input.Seek(pos, SeekOrigin.Begin);

                int length;
                while (true)
                {
                    length = ReadBeInt32(2);
                    if (length == 0)
                    {
                        break;
                    }
                    input.Seek(length, SeekOrigin.Current);
                }
                int nameLen = ReadBeInt32(1);
                if (nameLen == 0)
                {
                    continue;
                }
                length = ReadBeInt32(4);
                string? name = Encoding.UTF8.GetString(input.ReadBytes(nameLen));
                var offset = input.Position;
                yield return new ArchiveEntry(name, offset, length);
                pos = offset + length;
            }
        }


        private Stream ReadStream(ArchiveEntry entry)
        {
            return new PartialStream(input, entry.Offset, entry.Length);
        }


        private int ReadBeInt32(int length)
        {
            var readLen = input.Read(_buffer, 0, length);
            var res = 0;
            for (int i = 0; i < readLen; i++)
            {
                res = (res << 8) + _buffer[i];
            }
            return res;
        }

        public void Dispose()
        {
            input.Dispose();
        }

       
    }
}
