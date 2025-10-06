using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class BluePoChElementScanner(string package) : IBundleParser
    {
        public bool IsReverse1999 => package.Contains("re1999");


        public IBundleBinaryReader OpenRead(string fullPath)
        {
            return Parse(File.OpenRead(fullPath), new FilePath(fullPath));
        }

        public IBundleBinaryReader Parse(Stream input, IFilePath sourcePath)
        {
            return new BundleBinaryReader(input, EndianType.BigEndian);
        }

    }
}
