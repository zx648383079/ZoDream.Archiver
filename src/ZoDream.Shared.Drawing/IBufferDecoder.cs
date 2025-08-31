using System;

namespace ZoDream.Shared.Drawing
{
    public interface IBufferDecoder
    {
        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="data"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns>返回RGBA</returns>
        public byte[] Decode(ReadOnlySpan<byte> data, int width, int height);
        /// <summary>
        /// 解码
        /// </summary>
        /// <param name="data"></param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="output"></param>
        /// <returns>输出的长度</returns>
        public int Decode(ReadOnlySpan<byte> data, int width, int height, Span<byte> output);
    }

    public interface IBufferEncoder
    {
        /// <summary>
        /// 编码
        /// </summary>
        /// <param name="data">RGBA</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public byte[] Encode(ReadOnlySpan<byte> data, int width, int height);
    }
}
