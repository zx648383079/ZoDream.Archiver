using System;
using System.IO;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ZoDream.BundleExtractor.Unity.BundleFiles;
using ZoDream.BundleExtractor.Unity.CompressedFiles;
using ZoDream.BundleExtractor.Unity.SerializedFiles;

namespace ZoDream.BundleExtractor
{
    public partial class UnityBundleScheme : IArchiveScheme
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
            var reader = OpenBundle(stream);
            if (reader is not null)
            {
                return reader;
            }
            IArchiveScheme[] schemes = [
                new BrotliFileScheme(),
                new GZipFileScheme(),
                new SerializedFileScheme(),
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

        public static IArchiveReader? OpenBundle(Stream stream, IArchiveOptions? options = null)
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
