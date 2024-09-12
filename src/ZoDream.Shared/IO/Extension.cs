using System;
using System.IO;
using System.Threading;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.IO
{
    public static class StreamExtension
    {
        public static long CopyTo(this Stream input, Stream output, long length)
        {
            var buffer = new byte[Math.Min(length, 1024 * 5)];
            var len = 0L;
            while (len < length)
            {
                var res = input.Read(buffer, 0, buffer.Length);
                if (res == 0)
                {
                    break;
                }
                output.Write(buffer, 0, res);
                len += res;
            }
            return len;
        }

        public static void ExtractToDirectory(this IArchiveReader reader, IReadOnlyEntry entry, string folder, Action<double>? progress = null, CancellationToken token = default)
        {
            var fileName = Path.Combine(folder, Path.GetFileName(entry.Name.Replace('/', '\\')));
            using var fs = File.Create(fileName);
            reader.ExtractTo(entry, fs);
            progress?.Invoke(1);
        }
    }
}
