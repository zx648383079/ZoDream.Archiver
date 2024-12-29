using System.IO;
using System.Threading.Tasks;

namespace ZoDream.Shared.Interfaces
{
    public interface IStorageFileEntry
    {
        public string Name { get; }
        public string Path { get; }

        public Task<Stream> OpenReadAsync();
        public Task<Stream> OpenWriteAsync();

        public Task LaunchAsync();
    }
}
