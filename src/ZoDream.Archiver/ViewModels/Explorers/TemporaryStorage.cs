using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Archiver.ViewModels
{
    public class TemporaryStorage(StorageFolder folder) : ITemporaryStorage
    {
        public TemporaryStorage()
            : this (ApplicationData.Current.TemporaryFolder)
        {
            
        }

        private readonly ConcurrentQueue<IDisposable> _items = [];
        private readonly ConcurrentQueue<StorageFile> _fileItems = [];

        public Task<IStorageFileEntry> CreateAsync()
        {
            return CreateFileAsync(Guid.NewGuid().ToString());
        }

        public async Task<Stream> CreateAsync(string guid)
        {
            var file = await folder.CreateFileAsync(guid);
            _fileItems.Enqueue(file);
            return await file.OpenStreamForWriteAsync();
        }

        public async Task<IStorageFileEntry> CreateFileAsync(string guid)
        {
            var file = await folder.CreateFileAsync(guid);
            _fileItems.Enqueue(file);
            return new StorageFileEntry(file);
        }

        public async Task ClearAsync()
        {
            while (_fileItems.TryDequeue(out var item))
            {
                await item.DeleteAsync();
            }
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
            }
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }
        #endregion

        public async void Dispose()
        {
            Clear();
            await ClearAsync();
        }
    }
}
