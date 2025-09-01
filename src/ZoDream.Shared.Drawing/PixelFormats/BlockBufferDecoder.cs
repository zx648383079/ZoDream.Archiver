using System;

namespace ZoDream.Shared.Drawing
{
    /// <summary>
    /// 块的处理
    /// </summary>
    public abstract class BlockBufferDecoder : IBufferDecoder
    {
        internal const int PixelSize = 4;
        /// <summary>
        /// 压缩成字节占用的个数
        /// </summary>
        protected virtual int BlockSize => 8;

        protected virtual int BlockWidth => 4;
        protected virtual int BlockHeight => 4;
        /// <summary>
        /// 压缩之后一个像素占用的字节数
        /// </summary>
        protected virtual int BlockPixelSize => PixelSize;

        internal int GetCompressedSize(int width, int height) => ((width) >> 2) * ((height) >> 2) * BlockSize;

        public byte[] Decode(ReadOnlySpan<byte> data, int width, int height)
        {
            var buffer = new byte[width * height * PixelSize];
            Decode(data, width, height, buffer);
            return buffer;
        }

        public int Decode(ReadOnlySpan<byte> data, int width, int height, Span<byte> output)
        {
            var decodedBlockSize = BlockWidth * BlockHeight * BlockPixelSize;
            Span<byte> buffer = stackalloc byte[decodedBlockSize];
            var inputOffset = 0;
            for (int y = 0; y < height; y += BlockHeight)
            {
                for (var x = 0; x < width; x += BlockWidth)
                {
                    DecodeBlock(data.Slice(inputOffset, BlockSize), buffer);
                    BlockCopyTo(buffer, output, x, y, width, height);
                    inputOffset += BlockSize;
                }
            }
            return inputOffset;
        }

        private void BlockCopyTo(ReadOnlySpan<byte> data, Span<byte> output, int x, int y, int width, int height)
        {
            for (var i = 0; i < BlockWidth; i++)
            {
                var toX = x + i;
                if (toX >= width)
                {
                    continue;
                }
                for (var j = 0; j < BlockHeight; j++)
                {
                    var toY = y + j;
                    if (toY >= height)
                    {
                        continue;
                    }
                    var index = GetBlockIndex(i, j);
                    CopyPixelTo(data, index, output[((toY * width + toX) * PixelSize)..]);
                }
            }
        }

        /// <summary>
        /// 把 block 上的一个字节复制进去
        /// </summary>
        /// <param name="data"></param>
        /// <param name="index"></param>
        /// <param name="output"></param>
        protected virtual void CopyPixelTo(ReadOnlySpan<byte> data, int index, Span<byte> output)
        {
            data.Slice(index * BlockPixelSize, Math.Min(BlockPixelSize, PixelSize)).CopyTo(output);
            if (BlockPixelSize < PixelSize)
            {
                output[PixelSize - 1] = byte.MaxValue; // Alpha
            }
        }

        /// <summary>
        /// 根据图像上的点转压缩块上点的顺序
        /// </summary>
        /// <param name="x">0~(BlockWidth-1)</param>
        /// <param name="y">0~(BlockHeight-1)</param>
        /// <returns>0~(BlockWidth * BlockHeight - 1)</returns>
        protected virtual int GetBlockIndex(int x, int y)
        {
            // 纵向排列
            // return x * BlockWidth + y;
            // 横向排列
            return y * BlockWidth + x;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="data">byte[BlockSize]</param>
        /// <param name="output">byte[BlockWidth * BlockHeight * PixelSize]</param>
        protected abstract void DecodeBlock(ReadOnlySpan<byte> data, Span<byte> output);
    }
}
