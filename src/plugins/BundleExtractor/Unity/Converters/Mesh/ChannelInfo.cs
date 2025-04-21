using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class ChannelInfoConverter : BundleConverter<ChannelInfo>
    {
        public override ChannelInfo Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new ChannelInfo
            {
                Stream = reader.ReadByte(),
                Offset = reader.ReadByte(),
                Format = reader.ReadByte(),
                Dimension = (byte)(reader.ReadByte() & 0xF)
            };
            return res;
        }
    }

}
