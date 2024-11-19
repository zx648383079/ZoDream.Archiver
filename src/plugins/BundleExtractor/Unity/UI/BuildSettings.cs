namespace ZoDream.BundleExtractor.Unity.UI
{
    public sealed class BuildSettings : UIObject
    {
        public string m_Version;

        public BuildSettings(UIReader reader) : base(reader)
        {
            var levels = reader.ReadArray(r => r.ReadString());

            var hasRenderTexture = reader.ReadBoolean();
            var hasPROVersion = reader.ReadBoolean();
            var hasPublishingRights = reader.ReadBoolean();
            var hasShadows = reader.ReadBoolean();

            m_Version = reader.ReadAlignedString();
        }
    }
}
