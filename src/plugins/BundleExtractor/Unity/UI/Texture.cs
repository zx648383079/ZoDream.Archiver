namespace ZoDream.BundleExtractor.Unity.UI
{
    public abstract class Texture : NamedObject
    {
        protected Texture(UIReader reader) : this(reader, true)
        {
            
        }

        protected Texture(UIReader reader, bool isReadable)
            : base(reader, isReadable)
        {
            if (!isReadable)
            {
                return;
            }
            var version = reader.Version;
            if (version.GreaterThanOrEquals(2017, 3)) //2017.3 and up
            {
                var m_ForcedFallbackFormat = reader.ReadInt32();
                var m_DownscaleFallback = reader.ReadBoolean();
                if (version.GreaterThanOrEquals(2020, 2)) //2020.2 and up
                {
                    var m_IsAlphaChannelOptional = reader.ReadBoolean();
                }
                reader.AlignStream();
            }
        }
    }
}
