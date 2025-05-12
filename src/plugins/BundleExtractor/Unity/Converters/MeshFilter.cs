using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class MeshFilterConverter : BundleConverter<MeshFilter>
    {
        public override MeshFilter? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var target = reader.Get<BuildTarget>();
            if (target == BuildTarget.NoTarget)
            {
                var m_ObjectHideFlags = reader.ReadUInt32();
                var m_PrefabParentObject = serializer.Deserialize<PPtr>(reader);
                var m_PrefabInternal = serializer.Deserialize<PPtr>(reader);
            }
            var res = new MeshFilter
            {
                GameObject = reader.ReadPPtr<GameObject>(serializer),
                Mesh = reader.ReadPPtr<Mesh>(serializer)
            };
            return res;
        }
    }
}
