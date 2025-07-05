using System.IO;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleStorage
    {
        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public Stream Open(string fullPath);
        public IBundleBinaryReader OpenRead(string fullPath);
        public IBundleBinaryReader OpenRead(Stream input, string fileName);
    }
}
