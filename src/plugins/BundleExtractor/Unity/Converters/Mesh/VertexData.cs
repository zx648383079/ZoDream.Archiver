using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class VertexDataConverter : BundleConverter<VertexData>
    {
        public override VertexData? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new VertexData();
            if (version.LessThan(2018))//2018 down
            {
                res.CurrentChannels = reader.ReadUInt32();
            }

            res.VertexCount = reader.ReadUInt32();

            if (version.GreaterThanOrEquals(4)) //4.0 and up
            {
                res.Channels = reader.ReadArray<ChannelInfo>(serializer);
            }

            if (version.LessThan(5)) //5.0 down
            {
                var numStreams = version.LessThan(4) ? 4 : reader.ReadInt32();
                res.Streams = reader.ReadArray<StreamInfo>(numStreams, serializer);

                if (version.LessThan(4)) //4.0 down
                {
                    GetChannels(res, version);
                }
            }
            else //5.0 and up
            {
                GetStreams(res, version);
            }

            res.DataSize = reader.ReadArray(r => r.ReadByte());
            reader.AlignStream();
            return res;
        }

        internal static void GetStreams(VertexData res, Version version)
        {
            if (res.Channels.Length == 0)
            {
                res.Streams = [];
                return;
            }
            for (int i = 0; i < res.Channels.Length; i++)
            {
                res.Channels[i].Dimension &= 0xF;
            }
            var items = new List<StreamInfo>();
            var streamCount = res.Channels.Max(x => x.Stream) + 1;
            uint offset = 0;
            for (int s = 0; s < streamCount; s++)
            {
                uint chnMask = 0;
                uint stride = 0;
                for (int chn = 0; chn < res.Channels.Length; chn++)
                {
                    var m_Channel = res.Channels[chn];
                    if (m_Channel.Stream == s)
                    {
                        if (m_Channel.Dimension > 0)
                        {
                            chnMask |= 1u << chn;
                            stride += m_Channel.Dimension * MeshHelper.GetFormatSize(MeshHelper.ToVertexFormat(m_Channel.Format, version));
                        }
                    }
                }
                items.Add(new StreamInfo
                {
                    ChannelMask = chnMask,
                    Offset = offset,
                    Stride = stride,
                    DividerOp = 0,
                    Frequency = 0
                });
                offset += res.VertexCount * stride;
                //static size_t AlignStreamSize (size_t size) { return (size + (kVertexStreamAlign-1)) & ~(kVertexStreamAlign-1); }
                offset = offset + (16u - 1u) & ~(16u - 1u);
            }
            res.Streams = [..items];
        }

        private void GetChannels(VertexData res, Version version)
        {
            res.Channels = new ChannelInfo[6];
            for (int i = 0; i < 6; i++)
            {
                res.Channels[i] = new();
            }
            for (var s = 0; s < res.Streams.Length; s++)
            {
                var m_Stream = res.Streams[s];
                var channelMask = new BitArray([(int)m_Stream.ChannelMask]);
                byte offset = 0;
                for (int i = 0; i < 6; i++)
                {
                    if (channelMask.Get(i))
                    {
                        var m_Channel = res.Channels[i];
                        m_Channel.Stream = (byte)s;
                        m_Channel.Offset = offset;
                        switch (i)
                        {
                            case 0: //kShaderChannelVertex
                            case 1: //kShaderChannelNormal
                                m_Channel.Format = 0; //kChannelFormatFloat
                                m_Channel.Dimension = 3;
                                break;
                            case 2: //kShaderChannelColor
                                m_Channel.Format = 2; //kChannelFormatColor
                                m_Channel.Dimension = 4;
                                break;
                            case 3: //kShaderChannelTexCoord0
                            case 4: //kShaderChannelTexCoord1
                                m_Channel.Format = 0; //kChannelFormatFloat
                                m_Channel.Dimension = 2;
                                break;
                            case 5: //kShaderChannelTangent
                                m_Channel.Format = 0; //kChannelFormatFloat
                                m_Channel.Dimension = 4;
                                break;
                        }
                        res.Channels[i] = m_Channel;
                        offset += (byte)(m_Channel.Dimension * MeshHelper.GetFormatSize(MeshHelper.ToVertexFormat(m_Channel.Format, version)));
                    }
                }
            }
        }
    }

}
