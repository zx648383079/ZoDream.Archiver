using System.IO;
using System.Threading.Tasks;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;

namespace ZoDream.QuickBMS
{
    public class QuickBMSScheme : IArchiveScheme, IBundleArchiveScheme
    {
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            throw new System.NotImplementedException();
        }

        public Task<IArchiveWriter> CreateAsync(Stream stream, IArchiveOptions? options = null)
        {
            throw new System.NotImplementedException();
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            return Open(new BundleBinaryReader(stream), fileName, options);
        }

        public IArchiveReader? Open(IBundleBinaryReader reader, IFilePath sourcePath, IArchiveOptions? options = null)
        {
            return Open(reader, sourcePath.Name, options);
        }

        private IArchiveReader? Open(IBundleBinaryReader reader, string fileName, IArchiveOptions? options = null)
        {
            var pos = reader.Position;
            var buffer = reader.ReadBytes(8);
            reader.Position = pos;
            if (fileName.EndsWith(".dat") && buffer.StartsWith(FafullfsReader.Signature))
            {
                return new FafullfsReader(reader, options);
            }
            return null;
        }

        public Task<IArchiveReader?> OpenAsync(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            return Task.FromResult(Open(stream, filePath, fileName, options));
        }
    }
}
