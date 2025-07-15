using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    /// <summary>
    /// @see https://github.com/28598519a/Aeon_AssetDecDL
    /// </summary>
    /// <param name="input"></param>
    public class EeabStream(Stream input, string fullPath) : DeflateStream(input)
    {
        private readonly ulong[] _keys = [567837916298265195uL, 11364903281198254620uL, 6213825822676150020uL, 1554861362852924550uL];
        private readonly byte[] _hashData = Encoding.ASCII.GetBytes(Sha1Hashed(Path.GetFileNameWithoutExtension(fullPath)));

        private readonly byte[] _buffer = new byte[16];
        private int _bufferSize = 0;
        private long _bufferPosition;

        private long _position = 0;
        private readonly long _length = input.Length - 8;

        public override long Length => _length;

        public override long Position {
            get => _position;
            set {
                Seek(value, SeekOrigin.Begin);
            }
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            var pos = origin switch
            {
                SeekOrigin.Current => Position + offset,
                SeekOrigin.End => Length + offset,
                _ => offset
            };
            if (pos < 0)
            {
                pos = 0;
            } else if (pos > Length)
            {
                pos = Length;
            }
            _position = pos;
            return pos;
        }

        private void Read(long position)
        {
            var chunkIndex = _position / 16;
            var chunkBegin = chunkIndex * 16;
            if (_bufferSize > 0 && _bufferPosition == chunkBegin)
            {
                return;
            }
            input.Seek(chunkBegin + 8, SeekOrigin.Begin);
            var buffer = new byte[16];
            _bufferSize = input.Read(buffer, 0, buffer.Length);
            _bufferPosition = chunkBegin;
            if (_bufferSize == 0)
            {
                return;
            }
            if (_bufferSize < buffer.Length) // 结尾剩余的部分
            {
                Array.Copy(buffer, _buffer, _bufferSize);
            } else
            {
                var mask = _keys[chunkIndex % _keys.Length];
                Shuffle16(_buffer, buffer, mask);
            }
            for (int i = 0; i < _bufferSize; i++)
            {
                var j = (chunkBegin + i) % _hashData.Length;
                _buffer[i] ^= _hashData[j];
            }
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            var res = 0;
            var begin = Position;
            while (res < count)
            {
                var pos = begin + res;
                Read(pos);
                if (_bufferSize == 0)
                {
                    break;
                }
                var bufferOffset = (int)(pos - _bufferPosition);
                var len = _bufferSize - bufferOffset;
                Array.Copy(_buffer, bufferOffset, buffer, offset + res, len);
                res += len;
            }
            return res;
        }



        private static string Sha1Hashed(string input)
        {
            return Convert.ToHexString(SHA1.HashData(Encoding.ASCII.GetBytes(input))).ToLower();
        }

        private static void Shuffle16(byte[] dst, byte[] src, ulong mask)
        {
            int offset = 0;
            int index = 0;
            while (offset < 64)
            {
                dst[index ++] = src[(mask >> (offset & 0x3F)) & 0xF];
                offset += 4;
            }
        }


    }
}
