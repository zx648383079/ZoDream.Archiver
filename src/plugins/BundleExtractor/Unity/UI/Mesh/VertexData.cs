using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class VertexData
    {
        public uint m_CurrentChannels;
        public uint m_VertexCount;
        public List<ChannelInfo> m_Channels;
        public List<StreamInfo> m_Streams;
        public byte[] m_DataSize;

        public VertexData(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();

            if (version.LessThan(2018))//2018 down
            {
                m_CurrentChannels = reader.ReadUInt32();
            }

            m_VertexCount = reader.ReadUInt32();

            if (version.GreaterThanOrEquals(4)) //4.0 and up
            {
                var m_ChannelsSize = reader.ReadInt32();
                m_Channels = [];
                for (int i = 0; i < m_ChannelsSize; i++)
                {
                    m_Channels.Add(new ChannelInfo(reader));
                }
            }

            if (version.LessThan(5)) //5.0 down
            {
                var numStreams = version.LessThan(4) ? 4 : reader.ReadInt32();
                m_Streams = [];
                for (int i = 0; i < numStreams; i++)
                {
                    m_Streams.Add(new StreamInfo(reader));
                }

                if (version.LessThan(4)) //4.0 down
                {
                    GetChannels(version);
                }
            }
            else //5.0 and up
            {
                GetStreams(version);
            }

            m_DataSize = reader.ReadArray(r => r.ReadByte());
            reader.AlignStream();
        }

        private void GetStreams(UnityVersion version)
        {
            m_Streams = [];
            if (m_Channels.Count == 0)
            {
                return;
            }
            var streamCount = m_Channels.Max(x => x.stream) + 1;
            uint offset = 0;
            for (int s = 0; s < streamCount; s++)
            {
                uint chnMask = 0;
                uint stride = 0;
                for (int chn = 0; chn < m_Channels.Count; chn++)
                {
                    var m_Channel = m_Channels[chn];
                    if (m_Channel.stream == s)
                    {
                        if (m_Channel.dimension > 0)
                        {
                            chnMask |= 1u << chn;
                            stride += m_Channel.dimension * MeshHelper.GetFormatSize(MeshHelper.ToVertexFormat(m_Channel.format, version));
                        }
                    }
                }
                m_Streams.Add(new StreamInfo
                {
                    channelMask = chnMask,
                    offset = offset,
                    stride = stride,
                    dividerOp = 0,
                    frequency = 0
                });
                offset += m_VertexCount * stride;
                //static size_t AlignStreamSize (size_t size) { return (size + (kVertexStreamAlign-1)) & ~(kVertexStreamAlign-1); }
                offset = offset + (16u - 1u) & ~(16u - 1u);
            }
        }

        private void GetChannels(UnityVersion version)
        {
            m_Channels = new List<ChannelInfo>(6);
            for (int i = 0; i < 6; i++)
            {
                m_Channels.Add(new ChannelInfo());
            }
            for (var s = 0; s < m_Streams.Count; s++)
            {
                var m_Stream = m_Streams[s];
                var channelMask = new BitArray(new[] { (int)m_Stream.channelMask });
                byte offset = 0;
                for (int i = 0; i < 6; i++)
                {
                    if (channelMask.Get(i))
                    {
                        var m_Channel = m_Channels[i];
                        m_Channel.stream = (byte)s;
                        m_Channel.offset = offset;
                        switch (i)
                        {
                            case 0: //kShaderChannelVertex
                            case 1: //kShaderChannelNormal
                                m_Channel.format = 0; //kChannelFormatFloat
                                m_Channel.dimension = 3;
                                break;
                            case 2: //kShaderChannelColor
                                m_Channel.format = 2; //kChannelFormatColor
                                m_Channel.dimension = 4;
                                break;
                            case 3: //kShaderChannelTexCoord0
                            case 4: //kShaderChannelTexCoord1
                                m_Channel.format = 0; //kChannelFormatFloat
                                m_Channel.dimension = 2;
                                break;
                            case 5: //kShaderChannelTangent
                                m_Channel.format = 0; //kChannelFormatFloat
                                m_Channel.dimension = 4;
                                break;
                        }
                        offset += (byte)(m_Channel.dimension * MeshHelper.GetFormatSize(MeshHelper.ToVertexFormat(m_Channel.format, version)));
                    }
                }
            }
        }
    }

}
