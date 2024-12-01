using System.Collections.Generic;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class ParserBindChannels
    {
        public List<ShaderBindChannel> m_Channels;
        public uint m_SourceMap;

        public ParserBindChannels(IBundleBinaryReader reader)
        {
            int numChannels = reader.ReadInt32();
            m_Channels = new List<ShaderBindChannel>();
            for (int i = 0; i < numChannels; i++)
            {
                m_Channels.Add(new ShaderBindChannel(reader));
            }
            reader.AlignStream();

            m_SourceMap = reader.ReadUInt32();
        }
    }

}
