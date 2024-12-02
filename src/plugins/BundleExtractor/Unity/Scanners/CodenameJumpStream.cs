﻿using System.IO;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.Unity.Scanners
{
    internal class CodenameJumpStream(Stream input) : DeflateStream(input)
    {
        private static readonly byte[] _keys = [0x6B, 0xC9, 0xAC, 0x0E, 0xE7, 0xD2, 0xB1, 0x99, 0x39, 0x59, 0x26, 0x56, 0x1B, 0x6C, 0xBB, 0xA4, 0x83, 0xC8, 0x79, 0x2E, 0x4B, 0xB2, 0x9D, 0x69, 0x35, 0xB8, 0x9A, 0xD6, 0xD5, 0x63, 0x95, 0x20, 0x14, 0x82, 0x1C, 0x7C, 0xD4, 0xA9, 0x15, 0x56, 0xC3, 0xC5, 0xD7, 0x21, 0x03, 0x4E, 0x4A, 0x34, 0x6B, 0x05, 0x2D, 0x0B, 0xE2, 0x7D, 0x7D, 0xD7, 0xB2, 0xAE, 0x9E, 0x56, 0x91, 0xBA, 0x81, 0x81, 0x0E, 0x08, 0x4D, 0xA0, 0x09, 0xB5, 0x60, 0x74, 0x58, 0x36, 0x89, 0x09, 0x19, 0x2C, 0x10, 0xB1, 0xD0, 0xA3, 0x4C, 0x36, 0xAA, 0x95, 0xBC, 0x10, 0x39, 0x30, 0x93, 0xE8, 0xAD, 0x38, 0x51, 0xAA, 0xCA, 0x08, 0x67, 0x03, 0x08, 0xD1, 0x20, 0x05, 0x27, 0x0B, 0x9D, 0xB1, 0x4B, 0x42, 0x98, 0x03, 0x5A, 0x49, 0x97, 0xB0, 0x2A, 0xB6, 0x3A, 0x2C, 0x33, 0xA3, 0x65, 0xC7, 0x7D, 0xB9, 0x41, 0xAD, 0xE7, 0x70, 0x59, 0x61, 0x82, 0x59, 0xC9, 0x5A, 0x0B, 0x13, 0x6D, 0x95, 0x31, 0x31, 0x23, 0x22, 0xD0, 0x51, 0x45, 0x59, 0x09, 0x57, 0xA2, 0x60, 0x3B, 0xCE, 0x9B, 0x6E, 0x22, 0x9E, 0x87, 0xBD, 0x83, 0x88, 0x73, 0xD0, 0x79, 0xD0, 0xAC, 0xDC, 0xE1, 0x6C, 0xB3, 0xA4, 0xCC, 0x98, 0x04, 0xE8, 0xB6, 0xBB, 0xAC, 0x21, 0xB9, 0x2A, 0x6E, 0x78, 0x01, 0xED, 0xC1, 0xA6, 0x79, 0xE0, 0x9B, 0x68, 0x7B, 0x8A, 0x25, 0xE4, 0x47, 0xBB, 0x5D, 0x2A, 0xC0, 0x5A, 0xDE, 0x31, 0xEC, 0x5C, 0xCE, 0x6D, 0xBE, 0x68, 0x1E, 0x93, 0x44, 0x89, 0x56, 0x68, 0x4C, 0x6E, 0xD0, 0x46, 0xB0, 0x97, 0xE4, 0x72, 0x23, 0xB5, 0x87, 0x18, 0xD5, 0x2D, 0xA9, 0x0E, 0x63, 0xAE, 0xCE, 0x4A, 0x69, 0xD0, 0xD1, 0x6B, 0xB0, 0x0C, 0x1A, 0xBD, 0xE3, 0x01, 0x45, 0x8B, 0x93, 0xD5, 0x83, 0x9C, 0xB7, 0x12, 0x6C, 0xD5];
        public override int Read(byte[] buffer, int offset, int count)
        {
            var pos = Position;
            var res = input.Read(buffer, offset, count);
            for (var i = 0; i < res; i++)
            {
                var j = pos + i;
                buffer[offset + i] ^= _keys[j % _keys.Length];
            }
            return res;
        }
    }
}