using System.IO;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class CiPaiBundleElementScanner(string package) : IBundleElementScanner, IBundleStorage
    {

        public bool IsPerpetualNovelty => package.Contains("wh");

        public Stream Open(string path)
        {
            if (IsPerpetualNovelty)
            {
                return DecryptPerpetualNovelty(File.OpenRead(path));
            }
            return File.OpenRead(path);
        }

        public bool TryRead(IBundleBinaryReader reader, object instance)
        {
            if (instance is IElementLoader l)
            {
                l.Read(reader);
                return true;
            }
            return false;
        }
    }
}
