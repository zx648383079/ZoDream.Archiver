using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Models;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class MonoBehaviourConverter : BundleConverter<MonoBehaviour>, IBundleExporter
    {
        public override MonoBehaviour? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            var res = new MonoBehaviour
            {
                GameObject = serializer.Deserialize<PPtr<GameObject>>(reader),
                IsEnabled = reader.ReadByte()
            };
            reader.AlignStream();
            res.Script = serializer.Deserialize<PPtr<MonoScript>>(reader);
            res.Name = reader.ReadAlignedString();
            return res;
        }

        public void SaveAs(string fileName, ArchiveExtractMode mode)
        {
            
        }
    }
}
