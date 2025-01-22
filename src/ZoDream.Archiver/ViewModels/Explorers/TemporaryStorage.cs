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

        public Task<IStorageFileEntry> CreateAsync()
        {
            return CreateAsync(Guid.NewGuid().ToString());
        }

        public async Task<IStorageFileEntry> CreateAsync(string guid)
        {
            return new StorageFileEntry(await folder.CreateFileAsync(guid));
        }

        public async Task ClearAsync()
        {
            foreach (var item in await folder.GetItemsAsync())
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
        }
        #endregion

        public async void Dispose()
        {
            Clear();
            await ClearAsync();
        }
    }
}
