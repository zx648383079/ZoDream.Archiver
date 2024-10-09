using System.IO;
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
    }
}
