using System;
using System.Collections.Generic;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class ResourceManagerConverter : BundleConverter<ResourceManager>
    {
        public override ResourceManager? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
            }
            return new()
            {
                Container = reader.ReadArray(_ => new KeyValuePair<string, PPtr>(reader.ReadAlignedString(), serializer.Deserialize<PPtr>(reader)))
            };
        }
    }
}
