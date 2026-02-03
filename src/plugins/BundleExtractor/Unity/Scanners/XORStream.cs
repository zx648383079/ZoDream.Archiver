using System;
using System.Buffers;
using System.IO;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    /// <summary>
    /// ^ 编解码
    /// </summary>
    /// <param name="input"></param>
    /// <param name="xorPad"></param>
    /// <param name="maxPosition">包含</param>
    public class XORStream(
        Stream input,
        byte[] xorPad, long maxPosition = 0) : DeflateStream(input)
    {
        public override int Read(byte[] buffer, int offset, int count)
        {
            var pos = Position;
            var read = input.Read(buffer, offset, count);
            for (var i = 0; i < read; i++)
            {
                var current = pos + i;
                if (maxPosition > 0 && current > maxPosition)
                {
                    break;
                }
                buffer[offset + i] ^= xorPad[current % xorPad.Length];
            }
            return read;
        }

        public static string Recognize(Stream input, byte[] target)
        {
            if (input.Length < target.Length)
            {
                return string.Empty;
            }
            var pos = input.Position;
            var buffer = ArrayPool<byte>.Shared.Rent(target.Length);
            try
            {
                input.ReadExactly(buffer, 0, target.Length);
                return Recognize(buffer, target);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer);
                input.Position = pos;
            }
        }

        public static string Recognize(
            ReadOnlySpan<byte> buffer, 
            ReadOnlySpan<byte> target)
        {
            var length = Math.Min(buffer.Length, target.Length);
            var key = ArrayPool<byte>.Shared.Rent(length);
            var isSame = true;
            for (int i = 0; i < length; i++)
            {
                key[i] = (byte)(buffer[i] ^ target[i]);
                if (i > 0 && key[i] != key[0])
                {
                    isSame = false;
                }
            }
            try
            {
                return isSame ? key[0].ToString("X") : Convert.ToHexString(key, 0, length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(key);
            }
        }
    }
}
