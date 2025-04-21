using System;
using System.Collections.Generic;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Object = UnityEngine.Object;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class AssetInfoConverter : BundleConverter<AssetInfo>
    {
        public override AssetInfo? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new AssetInfo
            {
                PreloadIndex = reader.ReadInt32(),
                PreloadSize = reader.ReadInt32(),
                Asset = serializer.Deserialize<PPtr<Object>>(reader)
            };
        }
    }

    internal sealed class AssetBundleConverter : BundleConverter<AssetBundle>
    {
        public override AssetBundle? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            var version = reader.Get<Version>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            var res = new AssetBundle
            {
                Name = reader.ReadAlignedString(),
                PreloadTable = reader.ReadArray(_ => serializer.Deserialize<PPtr<Object>>(reader)),

                Container = reader.ReadArray(_ => new KeyValuePair<string, AssetInfo>(reader.ReadAlignedString(),
                        serializer.Deserialize<AssetInfo>(reader)))
            };
            return res;
        }
    }
}
