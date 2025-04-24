using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class TuanJieElementScanner(string package, IBundleOptions options) : IBundleStorage,
        IBundleCodec
    {
        public bool IsFakeHeader => package.Contains("fake");
        public bool IsGuiLongChao => package.Contains("glc");

        public Stream Open(string fullPath)
        {
            return File.OpenRead(fullPath);
        }

        public IBundleBinaryReader OpenRead(string fullPath)
        {
            return OpenRead(Open(fullPath), fullPath);
        }

        public IBundleBinaryReader OpenRead(Stream input, string fileName)
        {
            if (IsFakeHeader && !FileNameHelper.IsCommonFile(fileName))
            {
                input = OtherBundleElementScanner.ParseFakeHeader(input);
            }
            return new BundleBinaryReader(input, EndianType.BigEndian);
        }

    }
}
