using System.IO;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;

namespace ZoDream.PdfInserter
{
    public class PdfScheme : IArchiveScheme
    {
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            return new PdfWriter(stream, options);
        }

        public bool IsReadable(Stream stream)
        {
            throw new System.NotImplementedException();
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IArchiveReader?> OpenAsync(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            return Task.FromResult(Open(stream, filePath, fileName, options));
        }

        public Task<IArchiveWriter> CreateAsync(Stream stream, IArchiveOptions? options = null)
        {
            return Task.FromResult(Create(stream, options));
        }
    }
}
