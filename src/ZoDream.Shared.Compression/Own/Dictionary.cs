using System;
using System.IO;

namespace ZoDream.Shared.Compression.Own
{
    public class OwnDictionary: IOwnKey
    {
        public OwnDictionary(Stream stream)
        {
            _reader = stream;
        }
        public OwnDictionary(string binFile)
        {
            _reader = File.OpenRead(binFile);
        }

        private readonly Stream _reader;

        public void Seek(long len, SeekOrigin origin)
        {
            var pos = origin switch
            {
                SeekOrigin.Current => _reader.Position + len,
                SeekOrigin.End => _reader.Length + len,
                _ => len
            };
            _reader.Seek(pos % _reader.Length, SeekOrigin.Begin);
        }

        public byte ReadByte()
        {
            var code = _reader.ReadByte();
            if (code < 0)
            {
                _reader.Seek(0, SeekOrigin.Begin);
                code = _reader.ReadByte();
            }
            return (byte)code;
        }


        public void WriteByte(byte[] buffer, int length)
        {
            _reader.Write(Convert(buffer, length));
        }

        public void Dispose()
        {
            _reader?.Dispose();
        }

        public static void Convert(string binFile, params string[] fileItems)
        {
            using var output = new FileStream(binFile, FileMode.Create);
            // output.WriteByte(3);
            var buffer = new byte[32 * 30];
            foreach (var item in fileItems)
            {
                if (!File.Exists(item))
                {
                    continue;
                }
                using var input = File.OpenRead(item);
                while (true)
                {
                    var c = input.Read(buffer, 0, buffer.Length);
                    if (c == 0)
                    {
                        break;
                    }
                    output.Write(Convert(buffer, c));
                }
            }
        }

        public static byte[] Convert(byte[] buffer, int length)
        {
            var partLength = 32;
            var blockLength = 5;
            var partCount = (int)Math.Ceiling((double)length / partLength);
            var target = new byte[partCount * blockLength];
            for (var j = 0; j < partCount; j++)
            {
                var val = 0L;
                var begin = partLength * j;
                for (var i = 0; i < partLength; i++)
                {
                    var index = begin + i;
                    val = val * 10L + (index >= length ? 0 : (buffer[index] - 48));
                }
                begin = blockLength * j;
                for (var i = 0; i < blockLength; i++)
                {
                    target[begin + i] = (byte)((val >> (8 * (blockLength - i - 1))) & 0xFF);
                }
            }
            return target;
        }
        public static byte[] ConvertBack(byte[] buffer, int length)
        {
            var partLength = 32;
            var blockLength = 5;
            var partCount = (int)Math.Ceiling((double)length / blockLength);
            var target = new byte[partCount * partLength];
            for (var j = 0; j < partCount; j++)
            {
                var val = 0L;
                var begin = blockLength * j;
                for (var i = 0; i < blockLength; i++)
                {
                    val += (buffer[begin + i] & 0xFF) << (8 * (blockLength - i - 1));
                }
                begin = partLength * j;
                for (var i = blockLength - 1; i >= 0; i--)
                {
                    target[begin + i] = (byte)(val % 10 + 48);
                    val /= 10;
                }
            }
            return target;
        }
    }
}
