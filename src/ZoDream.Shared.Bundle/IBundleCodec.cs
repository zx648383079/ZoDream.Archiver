using System.IO;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleCodec
    {
        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="input"></param>
        public void Initialize(IBundleBinaryReader input);
        public IBundleBinaryReader Decode(IBundleBinaryReader input, BundleCodecType codecType, long compressedSize, long uncompressedSize);
        public Stream Decode(Stream input, BundleCodecType codecType, long compressedSize, long uncompressedSize);
    }
}
