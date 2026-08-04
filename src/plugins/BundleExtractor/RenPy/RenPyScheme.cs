using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using ZoDream.BundleExtractor.Xna;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.RenPy
{
    public class RenPyScheme(IEntryService service, IBundleOptions options) : IResourceScheme
    {
        public IBundleHandler? Open(string fileName)
        {
            if (!fileName.EndsWith(".rpa"))
            {
                return null;
            }
            return Open(File.OpenRead(fileName), fileName);
        }

        public IBundleHandler? Open(Stream stream, string fileName)
        {
            return new RenPyReader(
                new BundleBinaryReader(stream, EndianType.LittleEndian, leaveOpen: false),
                Path.GetFileName(fileName), service, options);
        }
    }
}
