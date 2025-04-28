using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class ParserBindChannelsConverter : BundleConverter<ParserBindChannels>
    {
        public override ParserBindChannels Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new ParserBindChannels
            {
                Channels = reader.ReadArray<ShaderBindChannel>(serializer)
            };

            reader.AlignStream();

            res.SourceMap = reader.ReadUInt32();
            return res;
        }
    }

}
