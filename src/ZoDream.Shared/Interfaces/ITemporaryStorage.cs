using System;
using System.IO;

namespace ZoDream.Shared.Interfaces
{
    /// <summary>
    /// 临时内容创建管理
    /// </summary>
    public interface ITemporaryStorage: IDisposable
    {

        public Stream Create();
        public Stream Create(string guid);

        /// <summary>
        /// 清除所有缓存文件
        /// </summary>
        public void Clear();
    }
}
