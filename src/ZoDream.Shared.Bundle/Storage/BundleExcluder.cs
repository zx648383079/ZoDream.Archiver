using System.Collections.Generic;

namespace ZoDream.Shared.Bundle
{
    public class BundleExcluder : IBundleExcluder
    {
        private readonly HashSet<string> _excludeItems = [];

        public void Exclude(string filePath, BundleExcludeFlag flag)
        {
            if ((flag & BundleExcludeFlag.Export) == BundleExcludeFlag.Export)
            {
                _excludeItems.Add(filePath);
            }
        }

        public bool IsExclude(string filePath)
        {
            return _excludeItems.Contains(filePath);
        }

        public bool IsExclude(string filePath, BundleExcludeFlag flag)
        {
            if ((flag & BundleExcludeFlag.Export) == BundleExcludeFlag.Export)
            {
                return IsExportable(filePath);
            }
            return IsExclude(filePath);
        }

        public bool IsExportable(string filePath)
        {
            return _excludeItems.Contains(filePath);
        }

        public void Reset()
        {
            _excludeItems.Clear();
        }
    }
}
