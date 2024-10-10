using SharpCompress.Readers;
using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ZoDream.Shared.Compression.Own;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.IO;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.Compression
{
    public partial class CompressScheme : IArchiveScheme
    {
        public IArchiveWriter Create(Stream stream, IArchiveOptions? options = null)
        {
            throw new NotImplementedException();
        }

        public bool IsReadable(Stream stream)
        {
            return true;
        }

        public IArchiveReader? Open(Stream stream, string filePath, string fileName, IArchiveOptions? options = null)
        {
            stream.Seek(0, SeekOrigin.Begin);
            var leaveStreamOpen = options?.LeaveStreamOpen ?? true;
            if (stream is not MultipartFileStream)
            {
                stream = TrySubvolumeStream(filePath, stream);
                leaveStreamOpen = leaveStreamOpen || stream is not MultipartFileStream;
            }
            if (options is null || options.LeaveStreamOpen != leaveStreamOpen)
            {
                options = new ArchiveOptions(options)
                {
                    LeaveStreamOpen = leaveStreamOpen
                };
            }
            var reader = new OwnArchiveScheme().Open(stream, filePath, fileName, options);
            if (reader is not null)
            {
                return reader;
            }
            var reader2 = ReaderFactory.Open(stream, CompressHelper.Convert(options));
            if (reader2 is not null)
            {
                return new CompressReader(reader2);
            }
            return null;
        }

        public static bool IsCryptographicException(Exception ex)
        {
            return ex is CryptographicException || ex is SharpCompress.Common.CryptographicException;
        }
        public Stream TrySubvolumeStream(string fileName, Stream fileStream)
        {
            var name = Path.GetFileName(fileName);
            var match = SubvolumeRegex().Match(name);
            if (!match.Success)
            {
                return fileStream;
            }
            // match.Groups[1].Value
            var folder = Path.GetDirectoryName(fileName)!;
            var globe = name[..(name.Length - match.Value.Length + 1)];
            string[] items;
            if (!string.IsNullOrEmpty(match.Groups[2].Value))
            {
                globe += "part*." + match.Groups[2].Value;
                items = [.. Directory.GetFiles(folder, globe)
                    .Where(i => PartRegex().IsMatch(i))
                    .Order()];
            } 
            else if (!string.IsNullOrEmpty(match.Groups[3].Value))
            {
                globe += match.Groups[3].Value + ".*";
                items = [.. Directory.GetFiles(folder, globe)
                    .Where(i => NumberEndRegex().IsMatch(i))
                    .Order()];
            }
            else
            {
                globe += "*";
                items = [.. Directory.GetFiles(folder, globe)
                    .Where(i => NumberPartRegex().IsMatch(i))
                    .OrderBy(i => {
                        var m = NumberPartRegex().Match(i);
                        if (string.IsNullOrEmpty(m.Groups[2].Value))
                        {
                            return 0;
                        }
                        return int.Parse(m.Groups[2].Value);
                    })];
            }
            if (items.Length <= 1)
            {
                return fileStream;
            }
            return MultipartFileStream.Open(items, fileName, fileStream);
        }

        [GeneratedRegex(@"(\.part\d+\.(rar|zip|7z)|\.(zip|7z|rar)\.\d+|\.(z|r)\d+|\.(zip))$")]
        private static partial Regex SubvolumeRegex();
        [GeneratedRegex(@"\.part(\d+)\.")]
        private static partial Regex PartRegex();

        [GeneratedRegex(@"\.(\d+)$")]
        private static partial Regex NumberEndRegex();
        [GeneratedRegex(@"\.([z|r](\d+)|zip|rar)$")]
        private static partial Regex NumberPartRegex();
    }
}
