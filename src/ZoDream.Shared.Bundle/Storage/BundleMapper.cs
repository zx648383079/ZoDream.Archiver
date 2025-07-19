using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace ZoDream.Shared.Bundle.Storage
{
    public class BundleMapper : Dictionary<string, string>, IBundleMapper
    {
        public bool TryGet(string fromPath, [NotNullWhen(true)] out string? toPath)
        {
            return TryGetValue(fromPath, out toPath);
        }
    }
}
