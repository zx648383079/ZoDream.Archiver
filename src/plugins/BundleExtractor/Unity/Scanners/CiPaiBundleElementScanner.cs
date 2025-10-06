using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class CiPaiBundleElementScanner(string package) : IBundleParser
    {

        public bool IsPerpetualNovelty => package.Contains("wh");

        public Stream Open(string fullPath)
        {
            //if (IsPerpetualNovelty)
            //{
            //    return DecryptPerpetualNovelty(File.OpenRead(fullPath));
            //}
            return File.OpenRead(fullPath);
        }

        public IBundleBinaryReader OpenRead(string fullPath)
        {
            return Parse(Open(fullPath), new FilePath(fullPath));
        }

        public IBundleBinaryReader Parse(Stream input, IFilePath sourcePath)
        {
            return new BundleBinaryReader(input, EndianType.BigEndian);
        }

    }
}
