using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Eastward
{
    public class EastwardScheme(IBundleSource fileItems) : IArchiveScheme, IBundleHandler
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
            var pos = stream.Position;
            var buffer = stream.ReadBytes(4);
            stream.Position = pos;
            if (fileName.EndsWith(".g") && GReader.IsSupport(buffer))
            {
                return new GReader(new EndianReader(stream, EndianType.LittleEndian, options?.LeaveStreamOpen == true));
            }
            if (HmgReader.IsSupport(buffer))
            {
                return new HmgReader(new BinaryReader(stream), fileName);
            }
            return null;
        }

        public Task<IArchiveReader?> OpenAsync(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            return Task.FromResult(Open(stream, filePath, fileName, options));
        }

        public void ExtractTo(string folder, ArchiveExtractMode mode, CancellationToken token = default)
        {
            foreach (var item in fileItems.GetFiles())
            {
                if (token.IsCancellationRequested)
                {
                    return;
                }
                var path = new FilePath(item);
                using var fs = fileItems.OpenRead(path);
                using var reader = Open(fs, path.FullPath, path.Name);
                if (reader is null)
                {
                    continue;
                }
                reader.ExtractToDirectory(folder, mode, null, token);
            }
        }

        public void Dispose()
        {
        }
    }
}
