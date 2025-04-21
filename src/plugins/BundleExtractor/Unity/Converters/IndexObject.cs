using System;
using System.Collections.Generic;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Index = UnityEngine.Index;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class IndexConverter : BundleConverter<Index>
    {
        public override Index? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                Object = serializer.Deserialize<PPtr>(reader),
                Size = reader.ReadUInt64()
            };
        }
    }

    internal sealed class IndexObjectConverter : BundleConverter<IndexObject>
    {
        public override IndexObject? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            return new IndexObject
            {
                Name = reader.ReadAlignedString(),
                AssetMap = reader.ReadArray(_ => new KeyValuePair<string, Index>(reader.ReadAlignedString(),
                        serializer.Deserialize<Index>(reader)))
            };
        }
    }
}
