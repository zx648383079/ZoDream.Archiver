using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public class JJKPhantomParadeStream : DeflateStream
    {
        public JJKPhantomParadeStream(Stream input) : base(input)
        {
            _baseStream = input;
            var key = input.ReadBytes(2);
            var signatureBytes = input.ReadBytes(13);
            var generation = input.ReadByte();

            for (var i = 0; i < 13; i++)
            {
                signatureBytes[i] ^= key[i % key.Length];
            }

            var signature = Encoding.UTF8.GetString(signatureBytes);
            if (signature != "_GhostAssets_")
            {
                throw new Exception("Invalid signature");
            }

            generation ^= (byte)(key[0] ^ key[1]);

            if (generation != 1)
            {
                throw new Exception("Invalid generation");
            }

            long value = 0;
            _basePosition = input.Position;
            var blockCount = (input.Length - _basePosition) / 0x10;

            using var writerMS = new MemoryStream();
            using var writer = new BinaryWriter(writerMS);
            for (int i = 0; i <= blockCount; i++)
            {
                if (i % 0x40 == 0)
                {
                    value = 0x64 * ((i / 0x40) + 1);
                }
                writer.Write(value);
                writer.Write((long)0);
                value ++;
            }

            using var aes = Aes.Create();
            aes.Key = "6154e00f9E9ce46dc9054E07173Aa546"u8.ToArray();
            aes.Mode = CipherMode.ECB;
            aes.Padding = PaddingMode.None;
            var encryptor = aes.CreateEncryptor();

            var keyBytes = writerMS.ToArray();
            _keys = encryptor.TransformFinalBlock(keyBytes, 0, keyBytes.Length);
        }

        private readonly Stream _baseStream;
        private readonly long _basePosition;
        private readonly byte[] _keys = new byte[0x40];

        public override long Length => _baseStream.Length - _basePosition;

        public override long Position {
            get => Math.Max(_baseStream.Position - _basePosition, 0);
            set {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var min = _basePosition;
            var max = _baseStream.Length;
            var pos = origin switch
            {
                SeekOrigin.Current => _baseStream.Position + offset,
                SeekOrigin.End => _baseStream.Length + offset,
                _ => _basePosition + offset,
            };
            return _baseStream.Seek(Math.Min(Math.Max(pos, min), max), SeekOrigin.Begin) - min;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var pos = Position;
            var res = _baseStream.Read(buffer, offset, count);
            for (var i = 0; i < res; i++)
            {
                var j = pos + i;
                buffer[offset + i] ^= _keys[j];
            }
            return res;
        }
    }
}
