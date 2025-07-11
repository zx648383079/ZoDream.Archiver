using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class PaperElementScanner(string package) : IBundleStorage
    {
        public bool IsLoveAndDeepSpace => package.Contains("deepspace");

        public bool IsShiningNikki => package.Contains(".nn4");


        public IBundleBinaryReader OpenRead(string fullPath)
        {
            return OpenRead(File.OpenRead(fullPath), new FilePath(fullPath));
        }

        public IBundleBinaryReader OpenRead(Stream input, IFilePath sourcePath)
        {
            return new BundleBinaryReader(input, EndianType.BigEndian);
        }

    }
}
