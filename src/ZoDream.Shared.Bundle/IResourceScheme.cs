using System.IO;

namespace ZoDream.Shared.Bundle
{
    /// <summary>
    /// 识别一个文件
    /// </summary>
    public interface IResourceScheme
    {
        public IBundleHandler? Open(string fileName);
        public IBundleHandler? Open(Stream stream, string fileName);
    }
}