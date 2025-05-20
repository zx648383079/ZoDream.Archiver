using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.IO
{
    public class StorageFolderEntry(string fileName) : IStorageFolderEntry
    {
        public string Name => Path.GetFileName(fileName);

        public string FullPath => fileName;

        public Task<Stream> CreateFileAsync(string name)
        {
            return Task.FromResult<Stream>(File.Create(Path.Combine(FullPath, name)));
        }

        public Task<Stream?> OpenFileAsync(string name)
        {
            var fileName = Path.Combine(FullPath, name);
            if (File.Exists(fileName))
            {
                return Task.FromResult<Stream?>(File.OpenRead(fileName));
            }
            return Task.FromResult<Stream?>(null);
        }

        public Task<IStorageFolderEntry> CreateFolderAsync(string name)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ExistsAsync(string name)
        {
            var fileName = Path.Combine(FullPath, name);
            return Task.FromResult(File.Exists(fileName) || Directory.Exists(fileName));
        }

        public Task<IStorageEntry?> GetAsync(string name)
        {
            var fileName = Path.Combine(FullPath, name);
            if (File.Exists(fileName))
            {
                return Task.FromResult<IStorageEntry?>(new StorageFileEntry(fileName));
            }
            if (Directory.Exists(fileName))
            {
                return Task.FromResult<IStorageEntry?>(new StorageFolderEntry(fileName));
            }
            return Task.FromResult<IStorageEntry?>(null);
        }

        public Task LaunchAsync()
        {
            Process.Start("explorer", $"/select,{fileName}");
            return Task.CompletedTask;
        }

       
    }
}
