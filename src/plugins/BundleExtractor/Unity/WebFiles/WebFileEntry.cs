using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.WebFiles
{
    public sealed class WebFileEntry(string name, long length) : ReadOnlyEntry(name, length)
    {
        public int Offset { get; private set; }
        public static WebFileEntry Read(IBundleBinaryReader reader)
        {
            return new(reader.ReadString(), reader.ReadInt32())
            {
                Offset = reader.ReadInt32(),
            };
        }



    }
}
