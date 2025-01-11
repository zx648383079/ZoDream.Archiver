using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.CompressedFiles
{
    public class BrotliFileScheme : IArchiveScheme, IBundleArchiveScheme
    {
        private static ReadOnlySpan<byte> BrotliSignature => "UnityWeb Compressed Content (brotli)"u8;
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public bool IsReadable(Stream stream)
        {
            var remaining = stream.Length - stream.Position;
            if (remaining < 4)
            {
                return false;
            }

            var position = stream.Position;

            stream.Position += 1;
            var bt = (byte)stream.ReadByte(); // read 3 bits
            int sizeBytes = bt & 0x3;

            if (stream.Position + sizeBytes > stream.Length)
            {
                stream.Position = position;
                return false;
            }

            int length = 0;
            for (int i = 0; i < sizeBytes; i++)
            {
                byte nbt = (byte)stream.ReadByte();  // read next 8 bits
                int bits = bt >> 2 | (nbt & 0x3) << 6;
                bt = nbt;
                length += bits << 8 * i;
            }

            if (length != BrotliSignature.Length
                || stream.Position + length > stream.Length)
            {
                stream.Position = position;
                return false;
            }

            var buffer = new byte[BrotliSignature.Length];
            stream.ReadExactly(buffer);
            stream.Position = position;
            return buffer.StartsWith(BrotliSignature);
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            if (!IsReadable(stream))
            {
                return null;
            }
            return new StreamArchiveReader(fileName,
                new BrotliStream(stream, CompressionMode.Decompress, options?.LeaveStreamOpen != false),
                options);
        }

        public IArchiveReader? Open(IBundleBinaryReader reader, string filePath, string fileName, IArchiveOptions? options = null)
        {
            return Open(reader.BaseStream, filePath, fileName, options);
        }

        public Task<IArchiveReader?> OpenAsync(Stream stream,
          string filePath,
          string fileName, IArchiveOptions? options = null)
        {
            return Task.FromResult(Open(stream, filePath, fileName, options));
        }

        public Task<IArchiveWriter> CreateAsync(Stream stream, IArchiveOptions? options = null)
        {
            return Task.FromResult(Create(stream, options));
        }
    }
}
