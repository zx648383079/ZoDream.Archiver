using System.IO;
using System.Threading.Tasks;

namespace ZoDream.Shared.Interfaces
{
    public interface IArchiveScheme
    {
        public IArchiveReader? Open(Stream stream,
            string filePath,
            string fileName, IArchiveOptions? options = null);

        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null);
        public Task<IArchiveReader?> OpenAsync(Stream stream, 
            string filePath, 
            string fileName, IArchiveOptions? options = null);

        public Task<IArchiveWriter> CreateAsync(Stream stream, IArchiveOptions? options = null);
    }
}
