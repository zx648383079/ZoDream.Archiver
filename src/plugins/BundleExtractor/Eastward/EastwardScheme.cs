using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using ZoDream.FModExporter;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Eastward
{
    public class EastwardScheme : IArchiveScheme
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
            if (fileName.EndsWith(".bank"))
            {
                return new RiffReader(stream, options);
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


        public IEnumerable<EastwardAssetBundle> Open(IBundleSource source)
        {
            foreach (var item in source.GetFiles("config.g"))
            {
                yield return new EastwardAssetBundle(source, item, this);
            }
        }

    }
}
