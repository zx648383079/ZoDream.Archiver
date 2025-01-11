using System;
using System.IO;
using System.Threading.Tasks;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor
{
    public partial class BundleScheme : IArchiveScheme
    {
        public IArchiveReader? Open(Stream stream,
            string filePath,
            string fileName, IArchiveOptions? options = null)
        {
            return new UnityBundleScheme().Open(stream, filePath, fileName, options);
        }

        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            throw new NotImplementedException();
        }


        public async Task<IArchiveReader?> OpenAsync(Stream stream,
          string filePath,
          string fileName, IArchiveOptions? options = null)
        {
            if (options is not IBundleOptions)
            {
                options = await Service.AskAsync<BundleOptions>();
            }
            return Open(stream, filePath, fileName, options);
        }

        public Task<IArchiveWriter> CreateAsync(Stream stream, IArchiveOptions? options = null)
        {
            return Task.FromResult(Create(stream, options));
        }
    }
}
