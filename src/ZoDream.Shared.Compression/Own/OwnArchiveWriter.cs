using System;
using System.IO;
using System.Linq;
using System.Text;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Compression.Own
{
    public class OwnArchiveWriter(Stream stream, IOwnKey key) : IArchiveWriter
    {
        public OwnArchiveWriter(Stream stream, IArchiveOptions options)
            : this(stream, OwnArchiveScheme.CreateKey(options))
        {
            _options = options;
            WriteHeader(true, true);
        }

        private readonly IArchiveOptions? _options;
        private bool _withName = false;
        private bool _multiple = false;
        private bool _nextPadding = false;
        private readonly int _maxBufferLength = 4096;

        public void WriteHeader(bool withName, bool multiple)
        {
            _withName = withName || multiple;
            _multiple = multiple;
            stream.WriteByte(0x23);
            stream.WriteByte(0x5A);
            if (multiple)
            {
                stream.WriteByte(3);
            }
            else
            {
                stream.WriteByte((byte)(withName ? 1 : 2));
            }
            stream.WriteByte(0x0A);
        }



        private void WriteName(string fileName)
        {
            var buffer = Encoding.UTF8.GetBytes(fileName);
            WriteLength(buffer.Length);
            WriteBytes(buffer, buffer.Length);
            _nextPadding = !_nextPadding;
        }

        private void WriteLength(long length)
        {
            if (length <= 250)
            {
                stream.WriteByte((byte)length);
                return;
            }
            var i = 0;
            var basic = 250;
            // 相加
            for (i = 251; i <= 252; i++)
            {
                var plus = i * (i - basic);
                if (length <= plus + 255)
                {
                    stream.WriteByte((byte)i);
                    stream.WriteByte((byte)(length - plus));
                    return;
                }
            }
            // 倍数
            basic = 252;
            i = 253;
            for (; i <= 255; i++)
            {
                var len = i - basic + 1;
                var buffer = new byte[len + 1];
                buffer[len] = (byte)i;
                var b = 0L;
                for (var j = len - 2; j >= 0; j--)
                {
                    b += (long)Math.Pow(i, j);
                }
                var rate = length - b;
                for (var j = 0; j < len; j++)
                {
                    buffer[j] = (byte)(rate % 256);
                    rate /= 256;
                }
                if (rate == 0)
                {
                    stream.Write(buffer.Reverse().ToArray());
                    break;
                }
            }

        }

        private void WriteBytes(byte[] buffer, int max)
        {
            for (int i = 0; i < max; i++)
            {
                var code = key.ReadByte();
                buffer[i] = (byte)OwnHelper.Clamp(
                    _nextPadding ? (buffer[i] + code) : (buffer[i] - code)
                    , 256);
            }
            stream.Write(buffer, 0, max);
        }

        private void WriteStream(Stream input)
        {
            var buffer = new byte[_maxBufferLength];
            var length = input.Length - input.Position;
            WriteLength(length);
            while (true)
            {
                var len = input.Read(buffer, 0, buffer.Length);
                if (len == 0)
                {
                    break;
                }
                WriteBytes(buffer, len);
            }
            _nextPadding = !_nextPadding;
        }

        public IReadOnlyEntry AddEntry(string name, string fullPath)
        {
            using var fs = File.OpenRead(fullPath);
            return AddEntry(name, fs);
        }

        public IReadOnlyEntry AddEntry(string name, Stream input)
        {
            if (_withName)
            {
                WriteName(name);
            }
            WriteStream(input);
            return new ReadOnlyEntry(name, input.Length);
        }

        public void Dispose()
        {
            key.Dispose();
            if (_options?.LeaveStreamOpen == false)
            {
                stream.Dispose();
            }
        }
    }
}
