using System.IO;
using System.Text;
using System.Threading.Tasks;
using ZoDream.Shared.Interfaces;

namespace ZoDream.ChmExtractor
{
    public class ChmScheme : IArchiveScheme
    {
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            throw new System.NotImplementedException();
        }

        public bool IsReadable(Stream stream)
        {
            var pos = stream.Position;
            var buffer = new byte[4];
            stream.ReadExactly(buffer);
            stream.Seek(pos, SeekOrigin.Begin);
            return Encoding.ASCII.GetString(buffer) == "ITSF";
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            if (!fileName.EndsWith(".chm") || !IsReadable(stream))
            {
                return null;
            }
            return new ChmReader(new BinaryReader(stream), options);
        }

        public Task<IArchiveReader?> OpenAsync(Stream stream,
              string filePath,
              string fileName, IArchiveOptions? options = null)
        {
            return Task.FromResult(Open(stream, filePath, fileName, options));
        }

        public Task<IArchiveWriter> CreateAsync(Stream stream, IArchiveOptions? options = null)
        {
            return Task.FromResult(Create(stream, options));
        }
    }
}
