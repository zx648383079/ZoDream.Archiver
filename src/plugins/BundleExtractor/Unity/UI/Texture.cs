using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ZoDream.BundleExtractor.Unity.UI
{
    public abstract class Texture : NamedObject
    {
        protected Texture(UIReader reader) : base(reader)
        {
            var version = reader.Version;
            if (version.GreaterThanOrEquals(2017, 3)) //2017.3 and up
            {
                var m_ForcedFallbackFormat = reader.Reader.ReadInt32();
                var m_DownscaleFallback = reader.Reader.ReadBoolean();
                if (version.GreaterThanOrEquals(2020, 2)) //2020.2 and up
                {
                    var m_IsAlphaChannelOptional = reader.Reader.ReadBoolean();
                }
                reader.Reader.AlignStream();
            }
        }
    }
}
