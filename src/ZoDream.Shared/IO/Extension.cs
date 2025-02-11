using System;
using System.Buffers;
using System.IO;
using System.Linq;
using System.Threading;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.Shared.IO
{
    public static class StreamExtension
    {
        /// <summary>
        /// 剩余部分全部读取出来
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static byte[] ToArray(this Stream input)
        {
            if (input is null || input.Length <= input.Position)
            {
                return [];
            }
            var buffer = new byte[input.Length - input.Position];
            input.ReadExactly(buffer);
            return buffer;
        }
        /// <summary>
        /// 停用缓存功能
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public static Stream StopCache(this Stream input)
        {
            if (input is CachedStream e)
            {
                return e.BaseStream;
            }
            return input;
        }


        public static byte[] ReadBytes(this Stream input, int length)
        {
            if (input is null || length <= 0)
            {
                return [];
            }
            var buffer = new byte[length];
            input.ReadExactly(buffer);
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
            var maxLength = (int)Math.Min(length, 1024 * 5);
            var buffer = ArrayPool<byte>.Shared.Rent(maxLength);
            var len = 0L;
            while (len < length)
            {
                var res = input.Read(buffer, 0, (int)Math.Min(maxLength, length - len));
                if (res == 0)
                {
                    break;
                }
                len += res;
            }
            ArrayPool<byte>.Shared.Return(buffer); 
        }
        /// <summary>
        /// 复制指定长度的内容
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="length"></param>
        /// <returns></returns>
        public static long CopyTo(this Stream input, Stream output, long length)
        {
            var maxLength = (int)Math.Min(length, 1024 * 5);
            var buffer = ArrayPool<byte>.Shared.Rent(maxLength);
            var len = 0L;
            while (len < length)
            {
                var res = input.Read(buffer, 0, (int)Math.Min(maxLength, length - len));
                if (res == 0)
                {
                    break;
                }
                output.Write(buffer, 0, res);
                len += res;
            }
            ArrayPool<byte>.Shared.Return(buffer);
            return len;
        }
        /// <summary>
        /// 读取并进行转换保存
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <param name="cb"></param>
        /// <returns></returns>
        public static long CopyTo(this Stream input, Stream output, 
            Func<byte[], byte[]> cb)
        {
            var length = input.Length - input.Position;
            var maxLength = (int)Math.Min(length, 1024 * 5);
            var buffer = ArrayPool<byte>.Shared.Rent(maxLength);
            var len = 0L;
            while (len < length)
            {
                var res = input.Read(buffer, 0, (int)Math.Min(maxLength, length - len));
                if (res == 0)
                {
                    break;
                }
                var compressed = res == maxLength ? buffer : buffer.Take(res).ToArray();
                var uncompressed = cb.Invoke(compressed);
                res = uncompressed.Length;
                if (res == 0)
                {
                    continue;
                }
                output.Write(uncompressed, 0, res);
                len += res;
            }
            ArrayPool<byte>.Shared.Return(buffer);
            return len;
        }

        public static void SaveAs(this Stream input, string fileName)
        {
            using var fs = File.Create(fileName);
            input.CopyTo(fs);
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

        /// <summary>
        /// 判断文件是否是以拓展名结束
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="extensionItems">拓展名包含 . 例如：.m.html</param>
        /// <returns></returns>
        public static bool HasExtension(this IReadOnlyEntry entry, params string[] extensionItems)
        {
            if (extensionItems.Length == 0)
            {
                return false;
            }
            var name = entry.Name;
            if (entry is ISourceEntry s)
            {
                if (s.IsDirectory)
                {
                    return false;
                }
                name = s.FullPath;
            }
            return HasExtension(name, extensionItems);
        }
        /// <summary>
        /// 判断文件是否是以拓展名结束
        /// </summary>
        /// <param name="entry"></param>
        /// <param name="extensionItems">拓展名包含 . 例如：.m.html</param>
        /// <returns></returns>
        public static bool HasExtension(this IStorageFileEntry entry, params string[] extensionItems)
        {
            return HasExtension(entry.FullPath, extensionItems);
        }

        public static bool HasExtension(string fileName, params string[] extensionItems)
        {
            if (extensionItems.Length == 0)
            {
                return false;
            }
            foreach (var item in extensionItems)
            {
                if (!string.IsNullOrWhiteSpace(item) &&
                    fileName.EndsWith(item, StringComparison.CurrentCultureIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
    }
}
