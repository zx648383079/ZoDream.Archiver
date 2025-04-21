using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class StreamInfoConverter : BundleConverter<StreamInfo>
    {
        public override StreamInfo Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new StreamInfo();
            res.ChannelMask = reader.ReadUInt32();
            res.Offset = reader.ReadUInt32();

            if (version.LessThan(4)) //4.0 down
            {
                res.Stride = reader.ReadUInt32();
                res.Align = reader.ReadUInt32();
            }
            else
            {
                res.Stride = reader.ReadByte();
                res.DividerOp = reader.ReadByte();
                res.Frequency = reader.ReadUInt16();
            }
            return res;
        }
    }

}
