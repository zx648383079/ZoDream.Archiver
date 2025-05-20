using System.IO;
using System.Threading.Tasks;

namespace ZoDream.Shared.Interfaces
{
    public interface IStorageEntry
    {
        public string Name { get; }
        public string FullPath { get; }

        public Task LaunchAsync();
    }
    public interface IStorageFolderEntry : IStorageEntry
    {
        public Task<bool> ExistsAsync(string name);
        public Task<IStorageEntry?> GetAsync(string name);
        public Task<Stream?> OpenFileAsync(string name);
        public Task<Stream> CreateFileAsync(string name);
        public Task<IStorageFolderEntry> CreateFolderAsync(string name);
    }

    public interface IStorageFileEntry : IStorageEntry
    {
        public Task<Stream> OpenReadAsync();
        public Task<Stream> OpenWriteAsync();
        /// <summary>
        /// 创建同级文件
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Task<Stream> CreateBrotherAsync(string name);

        
    }
}
