using System.IO;
using ZoDream.Shared;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Addressables
{
    public class CatalogBinaryReader(IBundleBinaryReader reader)
    {

        public CatalogBinaryReader(Stream input)
            : this(new BundleBinaryReader(input, EndianType.LittleEndian))
        {
            
        }



        private void ReadHeader()
        {
            var magic = reader.ReadInt32();
            var version = reader.ReadInt32();
            Expectation.ThrowIfNotVersion(version is not 1 and not 2);
            var keysOffset = reader.ReadUInt32();
            var idOffset = reader.ReadUInt32();
            var instanceProviderOffset = reader.ReadUInt32();
            var sceneProviderOffset = reader.ReadUInt32();
            var initObjectsArrayOffset = reader.ReadUInt32();
            if (version == 1 && keysOffset == 0x20)
            {
                var buildResultHashOffset = uint.MaxValue;
            }
            else
            {
                var buildResultHashOffset = reader.ReadUInt32();
            }
        }
    }
}
