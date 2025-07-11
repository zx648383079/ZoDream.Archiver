﻿using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal partial class TuanJieElementScanner(string package, IBundleOptions options) : IBundleStorage,
        IBundleCodec
    {
        public bool IsFakeHeader => package.Contains("fake");
        public bool IsGuiLongChao => package.Contains("glc");

        public IBundleBinaryReader OpenRead(string fullPath)
        {
            return OpenRead(File.OpenRead(fullPath), new FilePath(fullPath));
        }

        public IBundleBinaryReader OpenRead(Stream input, IFilePath sourcePath)
        {
            if (IsFakeHeader && !FileNameHelper.IsCommonFile(sourcePath.Name))
            {
                input = OtherBundleElementScanner.ParseFakeHeader(input);
            }
            return new BundleBinaryReader(input, EndianType.BigEndian);
        }

    }
}
