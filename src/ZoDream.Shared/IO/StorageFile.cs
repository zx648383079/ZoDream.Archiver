using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.IO
{
    public class StorageFileEntry(string fileName) : IStorageFileEntry
    {
        public string Name => Path.GetFileName(fileName);

        public string FullPath => fileName;

        public Task<Stream> OpenReadAsync()
        {
            return Task.FromResult<Stream>(File.OpenRead(fileName));
        }

        public Task<Stream> OpenWriteAsync()
        {
            return Task.FromResult<Stream>(File.OpenWrite(fileName));
        }

        public Task LaunchAsync()
        {
            Process.Start("explorer", $"/select,{fileName}");
            return Task.CompletedTask;
        }
    }
}
