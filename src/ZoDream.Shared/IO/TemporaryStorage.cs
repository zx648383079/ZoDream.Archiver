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
        private readonly ConcurrentQueue<IDisposable> _items = [];

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

        #region Memory
        public Stream Create()
        {
            var ms = new MemoryStream();
            Add(ms);
            return ms;
        }

        public void Add(IDisposable instance)
        {
            _items.Enqueue(instance);
        }

        public void Clear()
        {
            while (_items.TryDequeue(out var item))
            {
                item.Dispose();
                if (item is PartialStream p)
                {
                    p.BaseStream.Dispose();
                }
            }
        }
        #endregion

        public void Dispose()
        {
            Clear();
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
