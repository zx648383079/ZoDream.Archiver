using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ZoDream.ElectronExtractor;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ElectronExtractor
{
    public class ElectronScheme : IArchiveScheme
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
            return buffer.SequenceEqual(AsarReader.Signature);
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            if (fileName.EndsWith(".asar") && IsReadable(stream))
            {
                return new AsarReader(new EndianReader(stream, EndianType.LittleEndian), options);
            }
            return null;
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
