using System.IO;

namespace ZoDream.Shared.Bundle
{
    /// <summary>
    /// 识别一个文件
    /// </summary>
    public interface IResourceScheme
    {
        public IBundleReader? Open(string fileName);
        public IBundleReader? Open(Stream stream, string fileName);
    }
}