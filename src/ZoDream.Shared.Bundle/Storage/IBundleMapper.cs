using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ZoDream.Shared.Bundle
{
    /// <summary>
    /// 文件映射
    /// </summary>
    public interface IBundleMapper
    {
        public int Count {  get; }

        public IEnumerable<string> Keys { get; }

        public void Add(string fromPath, string toPath);
        public bool TryGet(string fromPath, [NotNullWhen(true)] out string? toPath);
    }
}
