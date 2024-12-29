using System;
using System.Threading.Tasks;

namespace ZoDream.Shared.Interfaces
{
    /// <summary>
    /// 临时内容创建管理
    /// </summary>
    public interface ITemporaryStorage: IDisposable
    {

        public Task<IStorageFileEntry> CreateAsync();
        public Task<IStorageFileEntry> CreateAsync(string guid);

        /// <summary>
        /// 清除所有缓存文件
        /// </summary>
        public Task ClearAsync();
    }
}
