using System.Collections.Generic;
using System.IO;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle
{
    public interface IBundleStorage
    {
        public Stream OpenRead(IFilePath filePath);
        public Stream OpenWrite(IFilePath filePath);

        public IEnumerable<string> GetFiles(params string[] searchPatternItems);
        public IEnumerable<string> GetDirectories(params string[] searchPatternItems);
        /// <summary>
        /// 获取所有的，不区分文件还是文件夹
        /// </summary>
        /// <param name="searchPatternItems"></param>
        /// <returns></returns>
        public IEnumerable<string> Glob(params string[] searchPatternItems);
    }
}
