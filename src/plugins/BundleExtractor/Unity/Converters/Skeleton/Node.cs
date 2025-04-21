using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class NodeConverter : BundleConverter<Node>
    {
        public override Node Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                ParentId = reader.ReadInt32(),
                AxesId = reader.ReadInt32()
            };
        }
    }
}
