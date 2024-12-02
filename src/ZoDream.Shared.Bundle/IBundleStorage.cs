using System.IO;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleStorage
    {
        /// <summary>
        /// 打开文件
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public Stream Open(string path);
    }
}
