using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Text;
using System.Threading;

namespace ZoDream.Shared.Bundle
{
    public partial class BundleStorage
    {
        internal static bool IsMatch(ReadOnlySpan<char> name, params string[] patternItems)
        {
            if (patternItems.Length == 0)
            {
                return true;
            }
            foreach (var item in patternItems)
            {
                if (FileSystemName.MatchesSimpleExpression(item, name, true))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsMatch(ReadOnlySpan<char> name, string pattern)
        {
            return string.IsNullOrEmpty(pattern) || FileSystemName.MatchesSimpleExpression(pattern, name, true);
        }

        /// <summary>
        /// 获取所有文件的数量
        /// </summary>
        /// <param name="items"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        public static uint FileCount(IEnumerable<string> items, CancellationToken token = default)
        {
            return FileCount(items, string.Empty, token);
        }
        public static uint FileCount(IEnumerable<string> items, 
            string pattern,
            IBundleFilter? filter,
            CancellationToken token = default)
        {
            var options = new EnumerationOptions()
            {
                RecurseSubdirectories = true,
                MatchType = MatchType.Win32,
                AttributesToSkip = FileAttributes.None,
                IgnoreInaccessible = false
            };
            var count = 0u;
            foreach (var item in items)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                if (File.Exists(item))
                {
                    count++;
                    continue;
                }
                var res = new FileSystemEnumerable<string>(item, delegate (ref FileSystemEntry entry)
                {
                    return entry.ToSpecifiedFullPath();
                }, options)
                {
                    ShouldIncludePredicate = delegate (ref FileSystemEntry entry)
                    {
                        return !entry.IsDirectory && IsMatch(entry.FileName, pattern);
                    }
                };
                foreach (var it in res)
                {
                    if (filter?.IsExclude(it) == true)
                    {
                        continue;
                    }
                    count++;
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
            return count;
        }
        public static uint FileCount(IEnumerable<string> items,
            string pattern,
            CancellationToken token = default)
        {
            var options = new EnumerationOptions()
            {
                RecurseSubdirectories = true,
                MatchType = MatchType.Win32,
                AttributesToSkip = FileAttributes.None,
                IgnoreInaccessible = false
            };
            var count = 0u;
            foreach (var item in items)
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }
                if (File.Exists(item))
                {
                    count++;
                    continue;
                }
                var res = new FileSystemEnumerable<byte>(item, delegate (ref FileSystemEntry entry)
                {
                    return 1;
                }, options)
                {
                    ShouldIncludePredicate = delegate (ref FileSystemEntry entry)
                    {
                        return !entry.IsDirectory && IsMatch(entry.FileName, pattern);
                    }
                };
                foreach (var _ in res)
                {
                    count++;
                    if (token.IsCancellationRequested)
                    {
                        break;
                    }
                }
            }
            return count;
        }

        public static IEnumerable<string> Glob(
            IEnumerable<string> fileItems,
            string[] searchPatternItems,
            SearchTarget target,
            CancellationToken token = default)
        {
            return Glob(fileItems, searchPatternItems, null, target, token);
        }
        public static IEnumerable<string> Glob(
            IEnumerable<string> fileItems, 
            string[] searchPatternItems,
            IBundleFilter? filter,
            SearchTarget target,
            CancellationToken token = default)
        {
            var options = new EnumerationOptions()
            {
                RecurseSubdirectories = true,
                MatchType = MatchType.Win32,
                AttributesToSkip = FileAttributes.None,
                IgnoreInaccessible = false
            };
            foreach (var item in fileItems)
            {
                if (File.Exists(item))
                {
                    if (target == SearchTarget.Files && IsMatch(Path.GetFileName(item), searchPatternItems))
                    {
                        yield return item;
                    }
                    continue;
                }
                var res = new FileSystemEnumerable<string>(item, delegate (ref FileSystemEntry entry)
                {
                    return entry.ToSpecifiedFullPath();
                }, options)
                {
                    ShouldIncludePredicate = delegate (ref FileSystemEntry entry)
                    {
                        if ((target == SearchTarget.Files && entry.IsDirectory)
                        || (target == SearchTarget.Directories && !entry.IsDirectory))
                        {
                            return false;
                        }
                        return IsMatch(entry.FileName, searchPatternItems);
                    }
                };
                foreach (var it in res)
                {
                    if (filter?.IsExclude(it) == true)
                    {
                        continue;
                    }
                    yield return it;
                }
            }
        }


        /// <summary>
        /// 请先排好序
        /// </summary>
        /// <param name="items"></param>
        /// <returns></returns>
        public static int ToHashCode(string[] items)
        {
            var buffer = BitConverter.GetBytes(items.Length);
            for (int i = items.Length - 1; i >= 0; i--)
            {
                var item = items[i];
                buffer[2] ^= (byte)(item.Length % 256);
                var buf = Encoding.UTF8.GetBytes(item);
                for (var j = 0; j < buf.Length; j++)
                {
                    buffer[(j * 3 + i) % buffer.Length] ^= buf[j];
                }
            }
            return BitConverter.ToInt32(buffer);
        }

        public static int ToHashCode(string value)
        {
            return ToHashCode([value]);
        }

        /// <summary>
        /// 生成一个文件内部的子路径虚拟路径
        /// </summary>
        /// <param name="source"></param>
        /// <param name="entry"></param>
        /// <returns></returns>
        public static string Combine(string source, string entry)
        {
            return $"{source}#{entry}";
        }
        /// <summary>
        /// 获取真实路径
        /// </summary>
        /// <param name="fullPath"></param>
        /// <returns></returns>
        public static string Separate(string fullPath)
        {
            return Separate(fullPath, out _);
        }
        /// <summary>
        /// 获取真实路径
        /// </summary>
        /// <param name="fullPath"></param>
        /// <param name="entry">内部子文件的文件名</param>
        /// <returns></returns>
        public static string Separate(string fullPath, out string entry)
        {
            var i = fullPath.LastIndexOf('#');
            if (i >= 0)
            {
                entry = fullPath[(i + 1)..];
                return fullPath[..i];
            }
            entry = string.Empty;
            return fullPath;
        }

        public static IEnumerable<IBundleEntrySource> LoadEntry(Stream input)
        {
            var reader = new BinaryReader(input);
            while (input.Position < input.Length)
            {
                var item = new BundleEntrySource(reader.ReadString());
                var count = reader.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    item.Add(new BundleEntry(reader.ReadString()));
                }
                count = reader.ReadInt32();
                for (var i = 0; i < count; i++)
                {
                    var id = reader.ReadInt64();
                    var type = reader.ReadInt32();
                    item.Add(new BundleEntry(id, reader.ReadString(), type));
                }
                count = reader.ReadInt32();
                item.LinkedItems = new long[count];
                for (var i = 0; i < count; i++)
                {
                    item.LinkedItems[i] = reader.ReadInt64();
                }
                count = reader.ReadInt32();
                item.LinkedPartItems = new string[count];
                for (var i = 0; i < count; i++)
                {
                    item.LinkedPartItems[i] = reader.ReadString();
                }
                yield return item;
            }
        }
    }
}
