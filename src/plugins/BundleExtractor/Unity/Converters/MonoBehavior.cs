using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class MonoBehaviourConverter : BundleConverter<MonoBehaviour>
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
                GameObject = reader.ReadPPtr<GameObject>(serializer),
                IsEnabled = reader.ReadByte()
            };
            reader.AlignStream();
            res.Script = reader.ReadPPtr<MonoScript>(serializer);
            res.Name = reader.ReadAlignedString();
            res.DataOffset = reader.Position;
            return res;
        }

    }
}
