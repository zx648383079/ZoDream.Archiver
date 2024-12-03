﻿using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

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

        public IBundleBinaryReader OpenRead(string path)
        {
            return OpenRead(Open(path));
        }

        public IBundleBinaryReader OpenRead(Stream input)
        {
            return new BundleBinaryReader(input, EndianType.LittleEndian);
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
