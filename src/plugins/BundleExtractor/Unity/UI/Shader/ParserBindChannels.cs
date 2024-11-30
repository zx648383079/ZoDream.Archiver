using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class ParserBindChannels
    {
        public List<ShaderBindChannel> m_Channels;
        public uint m_SourceMap;

        public ParserBindChannels(UIReader reader)
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
