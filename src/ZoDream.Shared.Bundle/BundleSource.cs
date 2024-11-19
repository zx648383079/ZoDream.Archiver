using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Enumeration;
using System.Linq;

namespace ZoDream.Shared.Bundle
{
    public class BundleSource(IEnumerable<string> fileItems) : IBundleSource
    {

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
                    yield return it;
                }
            }
        }

        public IEnumerable<IBundleChunk> EnumerateChunk()
        {
            return fileItems.Select(i => new BundleChunk(i));
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

        private static bool IsMatch(ReadOnlySpan<char> name, params string[] patternItems)
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
    }

    public enum SearchTarget
    {
        Files = 1,
        Directories,
        Both
    }
}
