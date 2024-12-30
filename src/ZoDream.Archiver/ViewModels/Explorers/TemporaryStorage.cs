using System;
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

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
