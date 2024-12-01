using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SpriteSettings
    {
        public uint settingsRaw;

        public uint packed;
        public SpritePackingMode packingMode;
        public SpritePackingRotation packingRotation;
        public SpriteMeshType meshType;

        public SpriteSettings(IBundleBinaryReader reader)
        {
            settingsRaw = reader.ReadUInt32();

            packed = settingsRaw & 1; //1
            packingMode = (SpritePackingMode)(settingsRaw >> 1 & 1); //1
            packingRotation = (SpritePackingRotation)(settingsRaw >> 2 & 0xf); //4
            meshType = (SpriteMeshType)(settingsRaw >> 6 & 1); //1
            //reserved
        }
    }

}
