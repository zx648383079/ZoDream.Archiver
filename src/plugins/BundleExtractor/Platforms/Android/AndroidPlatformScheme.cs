using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZoDream.BundleExtractor.Producers;

namespace ZoDream.BundleExtractor.Platforms
{
    public class AndroidPlatformScheme: IPlatformScheme
    {
        private const string AssetName = "assets";
        private const string MetaName = "META-INF";
        private const string BinName = "bin";
        private const string Il2CppGameAssemblyName = "libil2cpp.so";
        private const string AndroidUnityAssemblyName = "libunity.so";

        public string Root { get; private set; } = string.Empty;
        public IProducerScheme Producer { get; private set; }
        public bool TryLoad(IEnumerable<string> fileItems)
        {
            foreach (var item in fileItems)
            {
                if (!Directory.Exists(item))
                {
                    continue;
                }
                var found = GetMatchFolder(item, AssetName, MetaName);
                if (!string.IsNullOrEmpty(found))
                {
                    Root = found;
                    break;
                }
            }
            if (!string.IsNullOrWhiteSpace(Root))
            {
                return false;
            }
            Producer = UnityBundleScheme.GetProducer(fileItems);
            return true;
        }

        private string GetMatchFolder(string entry, params string[] folderNames)
        {
            var fileName = Path.GetFileName(entry);
            string parent;
            if (folderNames.Contains(fileName))
            {
                parent = Path.GetFullPath(Path.GetDirectoryName(entry));
                if (IsMatchFolder(parent, folderNames, fileName))
                {
                    return parent;
                }
            }
            foreach (var item in folderNames)
            {
                var items = Directory.GetDirectories(item, item, SearchOption.AllDirectories);
                foreach (var it in items)
                {
                    parent = Path.GetFullPath(Path.GetDirectoryName(it));
                    if (IsMatchFolder(parent, folderNames, item))
                    {
                        return parent;
                    }
                }
            }
            return string.Empty;
        }

        private bool IsMatchFolder(string folder, string[] folderNames, string skipName)
        {
            var success = true;
            foreach (var item in folderNames)
            {
                if (skipName == item)
                {
                    continue;
                }
                if (!Directory.Exists(Path.Combine(folder, item)))
                {
                    success = false;
                }
            }
            return success;
        }
    }
}
