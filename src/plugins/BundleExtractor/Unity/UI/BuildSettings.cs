using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal sealed class BuildSettings(UIReader reader) : UIObject(reader)
    {
        public string m_Version;

        public override void Read(IBundleBinaryReader reader)
        {
            base.Read(reader);
            var levels = reader.ReadArray(r => r.ReadString());

            var hasRenderTexture = reader.ReadBoolean();
            var hasPROVersion = reader.ReadBoolean();
            var hasPublishingRights = reader.ReadBoolean();
            var hasShadows = reader.ReadBoolean();

            m_Version = reader.ReadAlignedString();
        }
    }
}
