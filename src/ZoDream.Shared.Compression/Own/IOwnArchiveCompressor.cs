using System.IO;

namespace ZoDream.Shared.Compression.Own
{
    internal interface IOwnArchiveCompressor
    {
        /// <summary>
        /// 加密流
        /// </summary>
        /// <param name="input"></param>
        /// <param name="length">欲写入长度</param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public Stream CreateInflator(Stream input, long length, bool padding);
        /// <summary>
        /// 加密流
        /// </summary>
        /// <param name="input"></param>
        /// <param name="length">欲读取长度</param>
        /// <param name="padding"></param>
        /// <returns></returns>
        public Stream CreateDeflator(Stream input, long length, bool padding);
    }
}
