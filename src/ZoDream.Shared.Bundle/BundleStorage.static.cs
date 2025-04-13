using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
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
        public static int FileCount(IEnumerable<string> items, CancellationToken token = default)
        {
            return FileCount(items, string.Empty, token);
        }
        public static int FileCount(IEnumerable<string> items, 
            string pattern,
            IBundleFilter? filter = null,
            CancellationToken token = default)
        {
            var options = new EnumerationOptions()
            {
                RecurseSubdirectories = true,
                MatchType = MatchType.Win32,
                AttributesToSkip = FileAttributes.None,
                IgnoreInaccessible = false
            };
            var count = 0;
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
        public static int FileCount(IEnumerable<string> items,
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
            var count = 0;
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
        public static int ToHashCode(string fileName)
        {
            return fileName.GetHashCode();
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
        public static int ToHashCode(IEnumerable<string> items)
        {
            var hash = new HashCode();
            foreach (string item in items)
            {
                hash.Add(item);
            }
            return hash.ToHashCode();
        }
    }
}
