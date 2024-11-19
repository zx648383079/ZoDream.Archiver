using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Platforms
{
    public class AndroidPlatformScheme: IBundlePlatform
    {
        internal const string PlatformName = "Android";

        public string AliasName => PlatformName;

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
            if (!match.Success)
            {
                return true;
            }
            options.DisplayName = GetString(entry, match.Groups[1].Value);
            return true;
        }

        private string GetString(string entry, string key)
        {
            if (!key.StartsWith('@'))
            {
                return key;
            }
            var args = key[1..].Split('/');
            var fileName = Path.Combine(Path.GetDirectoryName(entry), "res", 
                "values", "strings.xml");
            if (!File.Exists(fileName))
            {
                return string.Empty;
            }
            var content = File.ReadAllText(fileName);
            var match = Regex.Match(content, @"\<string\s+name="""+ args[1] + @""">(.+?)\</string");
            if (!match.Success)
            {
                return string.Empty;
            }
            return match.Groups[1].Value;
        }
    }
}
