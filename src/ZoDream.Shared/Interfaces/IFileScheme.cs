using System.IO;
using System.Threading.Tasks;

namespace ZoDream.Shared.Interfaces
{
    public interface IEntryScheme<T>
    {
        public IEntryReader? GetReader(Stream input, IStorageFileEntry entry);
        public IEntryWriter? GetWriter(IStorageFileEntry entry);

        public Task<T?> OpenAsync(IStorageFileEntry entry, IArchiveOptions? options = null);

        public Task CreateAsync(IStorageFileEntry entry, T data, IArchiveOptions? options = null);
    }

    public interface IEntryReader
    {

    }

    public interface IEntryReader<T> : IEntryReader
    {
        public Task<T?> ReadAsync(IStorageFileEntry entry);
    }

    public interface IEntryWriter
    {

    }

    public interface IEntryWriter<T> : IEntryWriter
    {
        public Task WriteAsync(IStorageFileEntry entry, T data);
    }
}
