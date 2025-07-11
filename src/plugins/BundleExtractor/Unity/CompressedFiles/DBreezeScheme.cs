using System;
using System.IO;
using System.Threading.Tasks;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Unity.CompressedFiles
{
    public class DBreezeScheme : IArchiveScheme, IBundleArchiveScheme
    {
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public Task<IArchiveWriter> CreateAsync(Stream stream, IArchiveOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            if (fileName == DBreezeReader.FileName)
            {
                return new DBreezeReader(stream);
            }
            return null;
        }

        public IArchiveReader? Open(IBundleBinaryReader reader, IFilePath sourcePath, IArchiveOptions? options = null)
        {
            return Open(reader.BaseStream, sourcePath.FullPath, sourcePath.Name, options);
        }

        public Task<IArchiveReader?> OpenAsync(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            return Task.FromResult(Open(stream, filePath, fileName, options));
        }
    }
}
