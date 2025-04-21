using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class TextureParameterConverter : BundleConverter<TextureParameter>
    {
        public override TextureParameter Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new TextureParameter
            {
                NameIndex = reader.ReadInt32(),
                Index = reader.ReadInt32(),
                SamplerIndex = reader.ReadInt32()
            };
            if (version.GreaterThanOrEquals(2017, 3)) //2017.3 and up
            {
                var m_MultiSampled = reader.ReadBoolean();
            }
            res.Dim = reader.ReadSByte();
            reader.AlignStream();
            return res;
        }
    }

}
