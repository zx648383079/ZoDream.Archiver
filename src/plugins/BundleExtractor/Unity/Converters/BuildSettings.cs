using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class BuildSettingsConverter : BundleConverter<BuildOptions>
    {
        public override BuildOptions? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
            }
            var levels = reader.ReadArray(r => r.ReadAlignedString());

            var hasRenderTexture = reader.ReadBoolean();
            var hasPROVersion = reader.ReadBoolean();
            var hasPublishingRights = reader.ReadBoolean();
            var hasShadows = reader.ReadBoolean();

            return new()
            {
                Version = reader.ReadAlignedString()
            };
        }
    }
}
