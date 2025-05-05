using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public class KuRoElementScanner(string package) : IBundleStorage
    {

        public bool IsPunishingGrayRaven => package.Contains("pgr");

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
            return new BundleBinaryReader(input, EndianType.BigEndian);
        }
    }
}
