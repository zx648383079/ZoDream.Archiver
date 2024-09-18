using ZoDream.BundleExtractor.BundleFiles;
using System;
using System.IO;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.BundleExtractor.Platforms;
using System.Collections.Generic;
using ZoDream.BundleExtractor.Producers;
using ZoDream.BundleExtractor.CompressedFiles;
using ZoDream.BundleExtractor.SerializedFiles;

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
            return new UnityBundleReader(this, platform, options);
        }

        internal static IPlatformScheme? GetPlatform(IEnumerable<string> fileItems)
        {
            IPlatformScheme[] platforms = [
                new WindowsPlatformScheme(),
                new AndroidPlatformScheme(),
                new IosPlatformScheme(),
            ];
            foreach (var item in platforms)
            {
                if (item.TryLoad(fileItems))
                {
                    return item;
                }
            }
            return null;
        }

        internal static IProducerScheme GetProducer(IEnumerable<string> fileItems)
        {
            IProducerScheme[] producers = [
                new MiHoYoProducer(),
                new PaperProducer()
            ];
            foreach (var item in producers)
            {
                if (item.TryLoad(fileItems))
                {
                    return item;
                }
            }
            return new DefaultProducer();
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
            if (reader is not null)
            {
                return reader;
            }
            IArchiveScheme[] schemes = [
                new SerializedFileScheme(),
                new BrotliFileScheme(),
                new GZipFileScheme()
                ];
            foreach (var scheme in schemes) 
            {
                reader = scheme.Open(stream, filePath, fileName, options);
                if (reader is not null)
                {
                    return reader;
                }
            }
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
