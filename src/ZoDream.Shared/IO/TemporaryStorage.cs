using System;
using System.IO;
using System.Text.RegularExpressions;
using ZoDream.Shared.Interfaces;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace ZoDream.Shared.IO
{
    public partial class TemporaryStorage(string folder) : ITemporaryStorage
    {
        public TemporaryStorage()
            : this(AppDomain.CurrentDomain.BaseDirectory)
        {
            
        }

        private readonly ConcurrentQueue<string> _fileItems = [];

        public Task<IStorageFileEntry> CreateAsync()
        {
            var fullPath = Path.Combine(folder, Guid.NewGuid().ToString());
            _fileItems.Enqueue(fullPath);
            return Task.FromResult<IStorageFileEntry>(new StorageFileEntry(fullPath));
        }

        public Task<IStorageFileEntry> CreateAsync(string guid)
        {
            var fullPath = Path.Combine(folder, SafePathRegex().Replace(guid, "_"));
            _fileItems.Enqueue(fullPath);
            return Task.FromResult<IStorageFileEntry>(new StorageFileEntry(fullPath));
        }

        public Task ClearAsync()
        {
            while (_fileItems.TryDequeue(out var fileName))
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            while (_fileItems.TryDequeue(out var fileName))
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
            }
        }

        [GeneratedRegex(@"[\\/.:\<\>\(\)]")]
        private static partial Regex SafePathRegex();
    }
}
