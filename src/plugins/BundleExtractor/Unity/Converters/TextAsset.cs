using System;
using UnityEngine;
using ZoDream.BundleExtractor.Unity.Exporters;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class TextAssetConverter : BundleConverter<TextAsset>, IBundleExporter
    {
        public override TextAsset? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            return new()
            {
                Name = reader.ReadAlignedString(),
                Script = reader.ReadAsStream()
            };
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            new RawExporter(this).SaveAs(fileName, mode);
        }
    }
}
