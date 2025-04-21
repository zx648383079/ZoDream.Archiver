using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class StreamingInfoConverter : BundleConverter<ResourceSource>
    {
        public override ResourceSource Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            return new()
            {
                Offset = version.Major >= 2020 ? reader.ReadInt64() : reader.ReadUInt32(),
                Size = reader.ReadUInt32(),
                Source = reader.ReadAlignedString()
            };
        }
    }
}
