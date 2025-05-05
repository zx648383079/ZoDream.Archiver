using System;
using System.IO;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ZoDream.BundleExtractor.Unity.BundleFiles;
using ZoDream.BundleExtractor.Unity.CompressedFiles;
using ZoDream.BundleExtractor.Unity.SerializedFiles;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor
{
    public partial class UnityBundleScheme : IArchiveScheme, IBundleArchiveScheme
    {

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
            return Open(new BundleBinaryReader(stream, EndianType.BigEndian), filePath, fileName, options);
        }

        public Task<IArchiveReader?> OpenAsync(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            return Task.FromResult(Open(stream, filePath, fileName, options));
        }

        public Task<IArchiveWriter> CreateAsync(Stream stream, IArchiveOptions? options = null)
        {
            return Task.FromResult(Create(stream, options));
        }

        public IArchiveReader? Open(IBundleBinaryReader reader, string filePath, string fileName, IArchiveOptions? options = null)
        {
            var r = OpenBundle(reader);
            if (r is not null)
            {
                return r;
            }
            IBundleArchiveScheme[] schemes = [
                new DBreezeScheme(),
                new BrotliFileScheme(),
                new GZipFileScheme(),
                new SerializedFileScheme(),
            ];
            foreach (var scheme in schemes) 
            {
                r = scheme.Open(reader, filePath, fileName, options);
                if (r is not null)
                {
                    return r;
                }
            }
            return null;
        }

        public static IArchiveReader? OpenBundle(Stream stream, IArchiveOptions? options = null)
        {
            return OpenBundle(new BundleBinaryReader(stream, EndianType.BigEndian));
        }
        public static IArchiveReader? OpenBundle(IBundleBinaryReader reader, IArchiveOptions? options = null)
        {
            var pos = reader.Position;
            var found = reader.TryReadStringZeroTerm(0x20, out var signature);
            reader.Position = pos;
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
                _ => null,//throw new NotSupportedException(signature)
            };
        }

        #endregion

        public static Stream OpenRead(string fileName)
        {
            var name = Path.GetFileName(fileName);
            if (!SplitFileRegex().IsMatch(name))
            {
                return File.OpenRead(fileName);
            }
            return OpenSplitStream(fileName);
        }

        private static Stream OpenSplitStream(string fileName)
        {
            var name = Path.GetFileName(fileName);
            var folder = Path.GetDirectoryName(fileName);
            var i = name.LastIndexOf('t');
            var items =
                Directory.GetFiles(folder, name[..i] + '*')
                .OrderBy(j => int.Parse(Path.GetFileName(j)[(i + 1)..])).ToArray();
            return MultipartFileStream.Open(items);
        }


        [GeneratedRegex(@"\.split\d+$")]
        private static partial Regex SplitFileRegex();
    }
}
