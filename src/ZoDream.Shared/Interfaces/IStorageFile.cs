using System.IO;
using System.Threading.Tasks;

namespace ZoDream.Shared.Interfaces
{
    public interface IStorageFileEntry
    {
        public string Name { get; }
        public string FullPath { get; }

        public Task<Stream> OpenReadAsync();
        public Task<Stream> OpenWriteAsync();
        /// <summary>
        /// 创建同级文件
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Task<Stream> CreateBrotherAsync(string name);

        public Task LaunchAsync();
    }
}
