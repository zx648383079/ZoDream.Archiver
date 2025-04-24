using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class CiPaiBundleElementScanner(string package) : IBundleStorage
    {

        public bool IsPerpetualNovelty => package.Contains("wh");

        public Stream Open(string fullPath)
        {
            if (IsPerpetualNovelty)
            {
                return DecryptPerpetualNovelty(File.OpenRead(fullPath));
            }
            return File.OpenRead(fullPath);
        }

        public IBundleBinaryReader OpenRead(string fullPath)
        {
            return OpenRead(Open(fullPath), fullPath);
        }

        public IBundleBinaryReader OpenRead(Stream input, string fileName)
        {
            return new BundleBinaryReader(input, EndianType.BigEndian);
        }

    }
}
