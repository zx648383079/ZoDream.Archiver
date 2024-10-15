using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ZoDream.Shared.Interfaces;

namespace ZoDream.BundleExtractor.Platforms
{
    public class AndroidPlatformScheme: IBundlePlatform
    {
        public AndroidPlatformScheme()
        {
            
        }

        public AndroidPlatformScheme(IBundleProducer producer, IBundleEngine engine)
        {
            Producer = producer;
            Engine = engine;
            Sync();
        }

        private const string ManifestName = "AndroidManifest.xml";
        private const string AssetName = "assets";
        private const string MetaName = "META-INF";
        private const string BinName = "bin";
        private const string Il2CppGameAssemblyName = "libil2cpp.so";
        private const string AndroidUnityAssemblyName = "libunity.so";

        public string Root { get; private set; } = string.Empty;
        public IBundleProducer Producer { get; private set; }

        public IBundleEngine Engine { get; private set; }

        public bool TryLoad(IEnumerable<string> fileItems)
        {
            foreach (var item in fileItems)
            {
                if (!Directory.Exists(item))
                {
                    continue;
                }
                if (TryGetPackage(item))
                {
                    break;
                }
                var found = GetMatchFolder(item, AssetName, MetaName);
                if (!string.IsNullOrEmpty(found))
                {
                    Root = found;
                    break;
                }
            }
            if (Producer is not null)
            {
                Producer.TryLoad(fileItems);
                Engine?.TryLoad(fileItems);
            }
            if (string.IsNullOrWhiteSpace(Root))
            {
                return false;
            }
            if (Producer is null)
            {
                Producer = BundleScheme.GetProducer(fileItems);
                Engine = BundleScheme.GetEngine(this, fileItems);
            }
            return true;
        }

        private void Sync()
        {
            if (Engine is IOfPlatform e)
            {
                e.Platform = this;
            }
        }

        private bool TryGetPackage(string entry)
        {
            foreach (var item in Directory.GetFiles(entry, ManifestName, SearchOption.AllDirectories))
            {
                Root = Path.GetDirectoryName(item);
                var content = File.ReadAllText(item);
                var match = Regex.Match(content, @"package=""([a-zA-Z\d\.-_]+)""");
                if (match.Success && BundleScheme.TryGet(
                    match.Groups[1].Value, out var producer, out var engine))
                {
                    Producer = producer;
                    Engine = engine;
                    Sync();
                    return true;
                }
            }
            return !string.IsNullOrWhiteSpace(Root);
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
                var items = Directory.GetDirectories(entry, item, SearchOption.AllDirectories);
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
