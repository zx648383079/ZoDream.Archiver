using System;
using System.Buffers.Binary;

namespace ZoDream.Shared.Drawing
{
    public abstract class SwapBufferDecoder : IBufferDecoder, IBufferEncoder
    {
        /// <summary>
        /// 一个颜色值占的字节数
        /// </summary>
        public virtual int ColorSize => 4;

        public byte[] Decode(byte[] data, int width, int height)
        {
            var size = width * height;
            var buffer = new byte[size * 4];
            var colorSize = ColorSize;
            for (var i = 0; i < size; i++)
            {
                Decode(data, i * colorSize, buffer, i * 4);
            }
            return buffer;
        }
        public byte[] Decode(ReadOnlySpan<byte> data, int width, int height)
        {
            var buffer = new byte[width * height * 4];
            Decode(data, width, height, buffer);
            return buffer;
        }

        public int Decode(ReadOnlySpan<byte> data, int width, int height, Span<byte> output)
        {
            var size = width * height;
            var colorSize = ColorSize;
            for (var i = 0; i < size; i++)
            {
                Decode(data, i * colorSize, output, i * 4);
            }
            return size * 4;
        }

        /// <summary>
        /// 解码一个颜色
        /// </summary>
        /// <param name="input"></param>
        /// <param name="inputOffset"></param>
        /// <param name="output"></param>
        /// <param name="outputOffset"></param>
        protected abstract void Decode(ReadOnlySpan<byte> input, int inputOffset, Span<byte> output, int outputOffset);

        public byte[] Encode(ReadOnlySpan<byte> data, int width, int height)
        {
            var colorSize = ColorSize;
            var size = width * height;
            var buffer = new byte[size * colorSize];
            for (var i = 0; i < size; i++)
            {
                Encode(data, i * 4, buffer, i * colorSize);
            }
            return buffer;
        }
        /// <summary>
        /// 编码一个颜色
        /// </summary>
        /// <param name="input"></param>
        /// <param name="inputOffset"></param>
        /// <param name="output"></param>
        /// <param name="outputOffset"></param>
        protected abstract void Encode(ReadOnlySpan<byte> input, int inputOffset, Span<byte> output, int outputOffset);

        
    }
    public class RGBASwapDecoder(ColorSwapper swapper) : SwapBufferDecoder
    {
        /// <summary>
        /// 一个颜色空间占的字节数
        /// </summary>
        public virtual int ColorSpaceSize => 1;
        public override int ColorSize => swapper.KeyCount * ColorSpaceSize;

        protected override void Decode(ReadOnlySpan<byte> input, int inputOffset, Span<byte> output, int outputOffset)
        {
            foreach (var pair in swapper)
            {
                Decode(pair.Key, input, inputOffset + pair.Key * ColorSpaceSize, output, outputOffset + pair.Value);
            }
            if (swapper.TryContainsValue(ColorChannel.A, out var to) && !swapper.ContainsKey(ColorChannel.A))
            {
                output[outputOffset + to] = byte.MaxValue;
            }
        }
        /// <summary>
        /// 解码一个颜色空间
        /// </summary>
        /// <param name="index"></param>
        /// <param name="input"></param>
        /// <param name="inputOffset"></param>
        /// <param name="output"></param>
        /// <param name="outputOffset"></param>
        protected virtual void Decode(int index, ReadOnlySpan<byte> input, int inputOffset, Span<byte> output, int outputOffset)
        {
            output[outputOffset] = input[inputOffset];
        }

        protected override void Encode(ReadOnlySpan<byte> input, int inputOffset, Span<byte> output, int outputOffset)
        {
            foreach (var pair in swapper)
            {
                Encode(pair.Key, input, inputOffset + pair.Value, output, outputOffset + pair.Key * ColorSpaceSize);
            }
        }
        /// <summary>
        /// 编码一个颜色空间
        /// </summary>
        /// <param name="index"></param>
        /// <param name="input"></param>
        /// <param name="inputOffset"></param>
        /// <param name="output"></param>
        /// <param name="outputOffset"></param>
        protected virtual void Encode(int index, ReadOnlySpan<byte> input, int inputOffset, Span<byte> output, int outputOffset)
        {
            output[outputOffset] = input[inputOffset];
        }
    }

    /// <summary>
    /// 半个字节
    /// </summary>
    /// <param name="maps"></param>
    public class NibbleSwapDecoder(ColorSwapper swapper) : SwapBufferDecoder
    {

        private readonly byte[] _buffer = new byte[swapper.KeyCount];
        public override int ColorSize => swapper.KeyCount / 2;

        protected override void Decode(ReadOnlySpan<byte> input, int inputOffset, Span<byte> output, int outputOffset)
        {
            var next = output[outputOffset..];
            for (var i = 0; i < swapper.KeyCount; i += 2)
            {
                var val = input[inputOffset + i / 2];
                swapper.Write(next, i, (byte)((val & 0x0F) << 4));
                swapper.Write(next, i + 1, (byte)(val & 0xF0));
            }
        }

        protected override void Encode(ReadOnlySpan<byte> input, int inputOffset, Span<byte> output, int outputOffset)
        {
            foreach (var pair in swapper)
            {
                _buffer[pair.Key] = input[inputOffset + pair.Value];
            }
            if (swapper.TryContainsKey(ColorChannel.A, out var to) && !swapper.ContainsValue(ColorChannel.A))
            {
                _buffer[to] = byte.MaxValue;
            }
            for (var i = 0; i < swapper.KeyCount; i += 2)
            {
                output[outputOffset + i / 2] = (byte)((_buffer[i] << 4) & 0x0F + _buffer[i + 1] & 0xf0);
            }
        }
    }

    public class FloatSwapDecoder(ColorSwapper swapper) : RGBASwapDecoder(swapper)
    {
        public override int ColorSpaceSize => 4;

        protected override void Decode(int index, ReadOnlySpan<byte> input, int inputOffset, Span<byte> output, int outputOffset)
        {
            output[outputOffset] = ColorConverter.FromFloatToByte(input[inputOffset..]);
        }

        protected override void Encode(int index, ReadOnlySpan<byte> input, int inputOffset, Span<byte> output, int outputOffset)
        {
            var buffer = BitConverter.GetBytes(input[inputOffset] / 255f);
            buffer.CopyTo(output.Slice(outputOffset, 4));
        }
    }

    public class ShortSwapDecoder(ColorSwapper swapper) : RGBASwapDecoder(swapper)
    {
        public override int ColorSpaceSize => 2;

        protected override void Decode(int index, ReadOnlySpan<byte> input, int inputOffset, Span<byte> output, int outputOffset)
        {
            output[outputOffset] = ColorConverter.From16BitTo8Bit(
                    BinaryPrimitives.ReadUInt16BigEndian(input[inputOffset..]));
        }

        protected override void Encode(int index, ReadOnlySpan<byte> input, int inputOffset, Span<byte> output, int outputOffset)
        {
            var buffer = BitConverter.GetBytes(ColorConverter.From8BitTo16Bit(input[inputOffset]));
            buffer.CopyTo(output.Slice(outputOffset, 2));
        }
    }

    public class HalfSwapDecoder(ColorSwapper swapper) : RGBASwapDecoder(swapper)
    {
        public override int ColorSpaceSize => 2;

        protected override void Decode(int index, ReadOnlySpan<byte> input, int inputOffset, Span<byte> output, int outputOffset)
        {
            output[outputOffset] = ColorConverter.FromHalfToByte(input[inputOffset..]);
        }

        protected override void Encode(int index, ReadOnlySpan<byte> input, int inputOffset, Span<byte> output, int outputOffset)
        {
            var buffer = BitConverter.GetBytes((Half)input[inputOffset]);
            buffer.CopyTo(output.Slice(outputOffset, 2));
        }
    }
}
