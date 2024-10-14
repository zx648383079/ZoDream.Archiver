namespace ZoDream.BundleExtractor.Unity.UI
{
    public sealed class BuildSettings : UIObject
    {
        public string m_Version;

        public BuildSettings(UIReader reader) : base(reader)
        {
            var levels = reader.Reader.ReadArray(r => r.ReadString());

            var hasRenderTexture = reader.Reader.ReadBoolean();
            var hasPROVersion = reader.Reader.ReadBoolean();
            var hasPublishingRights = reader.Reader.ReadBoolean();
            var hasShadows = reader.Reader.ReadBoolean();

            m_Version = reader.ReadAlignedString();
        }
    }
}
