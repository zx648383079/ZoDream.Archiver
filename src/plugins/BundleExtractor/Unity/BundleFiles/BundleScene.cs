﻿using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.BundleFiles
{
    public class BundleScene
    {
        public uint CompressedSize { get; private set; }
        public uint DecompressedSize { get; private set; }

        public static BundleScene Read(IBundleBinaryReader reader)
        {
            return new()
            {
                CompressedSize = reader.ReadUInt32(),
                DecompressedSize = reader.ReadUInt32()
            };
        }
    }
}
