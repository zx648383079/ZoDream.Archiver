using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ZoDream.BundleExtractor.SerializedFiles;
using ZoDream.Shared.IO;

namespace ZoDream.BundleExtractor.UI
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
