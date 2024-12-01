using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SecondarySpriteTexture
    {
        public PPtr<Texture2D> texture;
        public string name;

        public SecondarySpriteTexture(IBundleBinaryReader reader)
        {
            texture = new PPtr<Texture2D>(reader);
            name = reader.ReadStringZeroTerm();
        }
    }
}
