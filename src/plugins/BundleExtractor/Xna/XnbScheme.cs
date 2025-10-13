using System.IO;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Interfaces;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Xna
{
    public class XnbScheme(IEntryService service, IBundleOptions options) : IResourceScheme
    {
        public IBundleHandler? Open(string fileName)
        {
            if (!fileName.EndsWith(".xnb"))
            {
                return null;
            }
            return Open(File.OpenRead(fileName), fileName);
        }

        public IBundleHandler? Open(Stream stream, string fileName)
        {
            return new XnbReader(new BundleBinaryReader(stream, EndianType.LittleEndian, leaveOpen: false), service, options);
        }
    }
}
