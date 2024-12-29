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

        public Task ClearAsync()
        {
            return Task.CompletedTask;
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }
    }
}
