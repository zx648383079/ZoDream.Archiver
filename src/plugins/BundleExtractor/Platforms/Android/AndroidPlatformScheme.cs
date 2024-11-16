using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Platforms
{
    public class AndroidPlatformScheme: IBundlePlatform
    {
        public const string PlatformName = "Android";

        private const string ManifestName = "AndroidManifest.xml";
        private const string AssetName = "assets";
        private const string MetaName = "META-INF";
        private const string BinName = "bin";

        public bool TryLoad(IBundleSource fileItems, IBundleOptions options)
        {
            foreach (var item in fileItems.GetFiles(ManifestName))
            {
                if (string.IsNullOrWhiteSpace(options.Platform))
                {
                    options.Platform = PlatformName;
                    options.Entrance = Path.GetDirectoryName(item);
                }
                if (TryGetPackage(item, options))
                {
                    return true;
                }
            }
            if (!string.IsNullOrWhiteSpace(options.Platform))
            {
                return true;
            }
            if (fileItems.GetDirectories(AssetName, MetaName).Any())
            {
                options.Platform = PlatformName;
                return true;
            }
            return true;
        }

        private bool TryGetPackage(string entry, IBundleOptions options)
        {
            var content = File.ReadAllText(entry);
            var match = Regex.Match(content, @"package=""([a-zA-Z\d\.-_]+)""");
            if (!match.Success)
            {
                return false;
            }
            options.Package = match.Groups[1].Value;
            BundleScheme.LoadWithPackage(options);
            if (!string.IsNullOrWhiteSpace(options.DisplayName))
            {
                return true;
            }
            match = Regex.Match(content, @"\<application[^\<\>]+?android:label=""(.+?)""");
            if (match.Success)
            {
                options.DisplayName = match.Groups[1].Value;
            }
            return true;
        }

    }
}
