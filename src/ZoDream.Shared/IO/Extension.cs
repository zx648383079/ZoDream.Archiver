using System;
using System.IO;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.IO
{
    public static class StreamExtension
    {
        public static byte[] ToArray(this Stream input)
        {
            var buffer = new byte[input.Length];
            input.Read(buffer, 0, buffer.Length);
            return buffer;
        }

        public static void Skip(this Stream input, long length)
        {
            if (length == 0)
            {
                return;
            }
            if (input.CanSeek)
            {
                input.Seek(length, SeekOrigin.Current);
                return;
            }
            if (length < 0)
            {
                throw new NotSupportedException(string.Empty);
            }
            var buffer = new byte[Math.Min(length, 1024 * 5)];
            var len = 0L;
            while (len < length)
            {
                var res = input.Read(buffer, 0, (int)Math.Min(buffer.Length, length - len));
                if (res == 0)
                {
                    break;
                }
                len += res;
            }
        }

        public static long CopyTo(this Stream input, Stream output, long length)
        {
            var buffer = new byte[Math.Min(length, 1024 * 5)];
            var len = 0L;
            while (len < length)
            {
                var res = input.Read(buffer, 0, (int)Math.Min(buffer.Length, length - len));
                if (res == 0)
                {
                    break;
                }
                output.Write(buffer, 0, res);
                len += res;
            }
            return len;
        }

        public static void ExtractTo(this IArchiveReader reader, IReadOnlyEntry entry, string fileName, Action<double>? progress = null, CancellationToken token = default)
        {
            using var fs = File.Create(fileName);
            reader.ExtractTo(entry, fs);
            progress?.Invoke(1);
        }
        public static void ExtractToDirectory(this IArchiveReader reader, IReadOnlyEntry entry, string folder, Action<double>? progress = null, CancellationToken token = default)
        {
            var fileName = Path.Combine(folder, Path.GetFileName(entry.Name.Replace('/', '\\')));
            reader.ExtractTo(entry, fileName, progress, token);
        }

        public static void ExtractToDirectory(this IArchiveReader reader, string folder, Action<double>? progressFn = null,
            CancellationToken token = default)
        {
            reader.ExtractToDirectory(folder, ArchiveExtractMode.Overwrite, progressFn, token);
        }
    }
}
