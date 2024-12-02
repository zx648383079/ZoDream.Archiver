using System;
using System.IO;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal class ProjectSekaiStream(Stream input) : DeflateStream(input)
    {
        private static readonly byte[] _keys = [0xFF, 0xFF, 0xFF, 0xFF, 0xFF, 0x00, 0x00, 0x00];
        private readonly long _basePosition = input.Position;

        public override long Length => input.Length - _basePosition;

        public override long Position {
            get => Math.Max(input.Position - _basePosition, 0);
            set {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var min = _basePosition;
            var max = input.Length;
            var pos = origin switch
            {
                SeekOrigin.Current => input.Position + offset,
                SeekOrigin.End => input.Length + offset,
                _ => _basePosition + offset,
            };
            return input.Seek(Math.Min(Math.Max(pos, min), max), SeekOrigin.Begin) - min;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var pos = Position;
            var res = input.Read(buffer, offset, count);
            for (var i = 0; i < res; i++)
            {
                var j = pos + i;
                if (j >= 0x80)
                {
                    break;
                }
                buffer[offset + i] ^= _keys[j % _keys.Length];
            }
            return res;
        }
    }
}