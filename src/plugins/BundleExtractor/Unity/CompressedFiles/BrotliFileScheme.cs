using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.CompressedFiles
{
    public class BrotliFileScheme : IArchiveScheme
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

            Span<byte> buffer = stackalloc byte[BrotliSignature.Length];
            stream.ReadExactly(buffer);
            stream.Position = position;
            return buffer.SequenceEqual(BrotliSignature);
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
    }
}
