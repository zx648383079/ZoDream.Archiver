using ZoDream.BundleExtractor.BundleFiles;
using System;
using System.IO;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.BundleExtractor.Platforms;
using System.Collections.Generic;

namespace ZoDream.BundleExtractor
{
    public class UnityBundleScheme : IBundleScheme, IArchiveScheme
    {
        #region BundleReader
        public IBundleReader? Load(IEnumerable<string> fileItems, IArchiveOptions? options = null)
        {
            var platform = GetPlatform(fileItems);
            if (platform == null)
            {
                return null;
            }
            return new UnityBundleReader(fileItems, platform, options);
        }

        private IPlatformScheme? GetPlatform(IEnumerable<string> fileItems)
        {
            var platform = new AndroidPlatformScheme();
            if (platform.TryLoad(fileItems))
            {
                return platform;
            }
            return null;
        }

        #endregion


        #region ArchiveReader
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public bool IsReadable(Stream stream)
        {
            throw new NotImplementedException();
        }
        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            var reader = OpenBundle(stream);

            return reader;
        }

        public IArchiveReader? OpenBundle(Stream stream, IArchiveOptions? options = null)
        {
            var pos = stream.Position;
            var reader = new EndianReader(stream, Shared.Models.EndianType.BigEndian);
            var found = reader.ReadStringZeroTerm(0x20, out var signature);
            stream.Position = pos;
            if (!found)
            {
                return null;
            }
            return signature switch
            {
                ArchiveBundleHeader.UnityArchiveMagic => new ArchiveBundleReader(reader, options),
                FileStreamBundleHeader.UnityFSMagic => new FileStreamBundleReader(reader, options),
                RawBundleHeader.UnityRawMagic => new RawWebBundleReader<RawBundleHeader>(reader, options),
                WebBundleHeader.UnityWebMagic => new RawWebBundleReader<WebBundleHeader>(reader, options),
                _ => throw new NotSupportedException(signature)
            };
        }

        #endregion

    }
}
