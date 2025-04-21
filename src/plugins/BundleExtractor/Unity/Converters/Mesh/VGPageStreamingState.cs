using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class VGPageStreamingStateConverter : BundleConverter<VGPageStreamingState>
    {
        public override VGPageStreamingState Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new VGPageStreamingState
            {
                BulkOffset = reader.ReadUInt32(),
                BulkSize = reader.ReadUInt32(),
                PageSize = reader.ReadUInt32(),
                DependenciesStart = reader.ReadUInt32(),
                DependenciesNum = reader.ReadUInt32(),
                Flags = reader.ReadUInt32()
            };
            return res;
        }
    }
}
