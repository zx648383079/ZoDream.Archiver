using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Storage;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Archiver.ViewModels
{
    public class StorageFileEntry(StorageFile file) : IStorageFileEntry
    {
        public string Name => file.Name;

        public string FullPath => file.Path;

        public async Task<Stream> OpenReadAsync()
        {
            return await file.OpenStreamForReadAsync();
        }

        public async Task<Stream> OpenWriteAsync()
        {
            return await file.OpenStreamForWriteAsync();
        }

        public async Task LaunchAsync()
        {
            _ = await Windows.System.Launcher.LaunchFileAsync(file);
        }
    }
}
