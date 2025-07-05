using System;
using System.IO;
using System.Threading.Tasks;

namespace ZoDream.Shared.Interfaces
{
    /// <summary>
    /// 临时内容创建管理
    /// </summary>
    public interface ITemporaryStorage: IDisposable
    {

        public Task<IStorageFileEntry> CreateAsync();
        public Task<IStorageFileEntry> CreateFileAsync(string guid);
        /// <summary>
        /// 创建临时读写流
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        public Task<Stream> CreateAsync(string guid);
        /// <summary>
        /// 创建内存流
        /// </summary>
        /// <returns></returns>
        public Stream Create();
        /// <summary>
        /// 添加需要手动销毁的内容
        /// </summary>
        /// <param name="instance"></param>
        public void Add(IDisposable instance);

        /// <summary>
        /// 清除所有缓存文件
        /// </summary>
        public Task ClearAsync();
        /// <summary>
        /// 清理临时创建的内存流
        /// </summary>
        public void Clear();
    }
}
