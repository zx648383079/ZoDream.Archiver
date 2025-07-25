﻿using System.IO;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal class XORStream(
        Stream input,
        byte[] xorPad, int maxPosition = 0) : DeflateStream(input)
    {
        public override int Read(byte[] buffer, int offset, int count)
        {
            var pos = Position;
            var read = input.Read(buffer, offset, count);
            for (var i = 0; i < read; i++)
            {
                var current = pos + i;
                if (maxPosition > 0 && current > maxPosition)
                {
                    break;
                }
                buffer[offset + i] ^= xorPad[current % xorPad.Length];
            }
            return read;
        }
    }
}
