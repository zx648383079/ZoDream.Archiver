using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;
using System.Threading;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle
{
    public class BundleSource(IEnumerable<string> fileItems) : IBundleSource
    {

        public BundleSource(IEnumerable<string> fileItems, IEntryService service)
            : this(fileItems)
        {
            _service = service;
        }

        private readonly IEntryService? _service;
        private readonly int _hasCode = ToHashCode(fileItems);
        private IBundleFilter? _filter;
        private uint _recordIndex;

        /// <summary>
        /// 获取文件的数量，必须先调用 Analyze 方法
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 重新计算文件的数量
        /// </summary>
        /// <returns></returns>
        public int Analyze(CancellationToken token = default)
        {
            _service?.TryLoadPoint(_hasCode, out _recordIndex);
            return Count = FileCount(fileItems, token);
        }

        public int Analyze(IBundleFilter filter, CancellationToken token = default)
        {
            _filter = filter;
            return Analyze(token);
        }

        public void Breakpoint()
        {
            _service?.SavePoint(_hasCode, _recordIndex);
        }

        public IEnumerable<string> GetFiles(params string[] searchPatternItems)
        {
            return Glob(searchPatternItems, SearchTarget.Files);
        }

        public IEnumerable<string> GetDirectories(params string[] searchPatternItems)
        {
            return Glob(searchPatternItems, SearchTarget.Directories);
        }

        public IEnumerable<string> Glob(params string[] searchPatternItems)
        {
            return Glob(searchPatternItems, SearchTarget.Both);
        }
        public IEnumerable<string> Glob(string[] searchPatternItems, SearchTarget target)
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
                    if (_filter?.IsExclude(it) == true)
                    {
                        continue;
                    }
                    yield return it;
                }
            }
        }

        public IEnumerable<IBundleChunk> EnumerateChunk()
        {
            return fileItems.Select(i => new BundleChunk(i));
        }

        public IEnumerable<IBundleChunk> EnumerateChunk(int maxFileCount)
        {
            var items = new List<string>();
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
                    yield return new BundleChunk(item);
                    continue;
                }
                var res = new FileSystemEnumerable<string>(item, delegate (ref FileSystemEntry entry)
                {
                    return entry.ToSpecifiedFullPath();
                }, options)
                {
                    ShouldIncludePredicate = delegate (ref FileSystemEntry entry)
                    {
                        return !entry.IsDirectory;
                    }
                };
                foreach (var it in res)
                {
                    if (_filter?.IsExclude(it) == true)
                    {
                        continue;
                    }
                    items.Add(it);
                    if (items.Count >= maxFileCount)
                    {
                        yield return new BundleChunk(item, [.. items]);
                        items.Clear();
                    }
                }
                if (items.Count > 0)
                {
                    yield return new BundleChunk(item, [.. items]);
                    items.Clear();
                }
            }
        }

        public IEnumerable<IBundleChunk> EnumerateChunk(IDependencyDictionary dependencies)
        {
            foreach (var item in GetFiles())
            {
                if (dependencies.TryGet(item, out var items))
                {
                    yield return new BundleChunk(dependencies.Entrance, [.. items, item]);
                }
                yield return new BundleChunk(item);
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return fileItems.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return fileItems.GetEnumerator();
        }

        public override int GetHashCode()
        {
            return _hasCode;
        }

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
        public static int FileCount(IEnumerable<string> items, string pattern, CancellationToken token = default)
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

        public static int ToHashCode(IEnumerable<string> items)
        {
            var hash = new HashCode();
            foreach (string item in items.Order())
            {
                hash.Add(item);
            }
            return hash.ToHashCode();
        }
    }

    public enum SearchTarget
    {
        Files = 1,
        Directories,
        Both
    }
}
