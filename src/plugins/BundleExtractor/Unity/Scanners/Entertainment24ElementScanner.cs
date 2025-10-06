using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    public partial class Entertainment24ElementScanner(string package) : IBundleParser
    {

        public bool IsNaraka => package.Contains("naraka");


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
