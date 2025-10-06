using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using ZoDream.Shared.Interfaces;

namespace ZoDream.Shared.Bundle
{
    public class BundleSource : IBundleSource
    {
        /// <summary>
        /// 限制一下单次依赖的数量，避免一些不必要的重复依赖
        /// </summary>
        internal const int CHUNK_MAX_DEPENDENCY = 20;
        public BundleSource(IEnumerable<string> fileItems)
        {
            _entryItems = [.. fileItems.Order()];
            _hasCode = BundleStorage.ToHashCode(_entryItems);
        }

        private readonly int _hasCode;
        private readonly string[] _entryItems;
        /// <summary>
        /// 获取文件的数量，必须先调用 Analyze 方法
        /// </summary>
        public uint Count { get; private set; }

        /// <summary>
        /// 重新计算文件的数量
        /// </summary>
        /// <returns></returns>
        public uint Analyze(CancellationToken token = default)
        {
            return Count = BundleStorage.FileCount(_entryItems, token);
        }


        public IEnumerable<string> GetFiles(params string[] searchPatternItems)
        {
            return BundleStorage.Glob(_entryItems, searchPatternItems, SearchTarget.Files);
        }

        public IEnumerable<string> GetDirectories(params string[] searchPatternItems)
        {
            return BundleStorage.Glob(_entryItems, searchPatternItems, SearchTarget.Directories);
        }

        public IEnumerable<string> Glob(params string[] searchPatternItems)
        {
            return BundleStorage.Glob(_entryItems, searchPatternItems, SearchTarget.Both);
        }

        public Stream OpenRead(IFilePath filePath)
        {
            return File.OpenRead(filePath.FullPath);
        }

        public Stream OpenWrite(IFilePath filePath)
        {
            return File.Create(filePath.FullPath);
        }

        public string GetRelativePath(string filePath)
        {
            return BundleStorage.GetRelativePath(_entryItems, filePath);
        }

        public override int GetHashCode()
        {
            return _hasCode;
        }
    }

    public enum SearchTarget
    {
        Files = 1,
        Directories,
        Both
    }
}
