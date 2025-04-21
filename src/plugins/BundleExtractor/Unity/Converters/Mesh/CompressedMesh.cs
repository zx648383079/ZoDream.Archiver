using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class CompressedMeshConverter : BundleConverter<CompressedMesh>
    {
        public override CompressedMesh? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new CompressedMesh();
            res.Vertices = serializer.Deserialize<PackedFloatVector>(reader);
            res.UV = serializer.Deserialize<PackedFloatVector>(reader);
            if (version.LessThan(5))
            {
                res.BindPoses = serializer.Deserialize<PackedFloatVector>(reader);
            }
            res.Normals = serializer.Deserialize<PackedFloatVector>(reader);
            res.Tangents = serializer.Deserialize<PackedFloatVector>(reader);
            res.Weights = serializer.Deserialize<PackedIntVector>(reader);
            res.NormalSigns = serializer.Deserialize<PackedIntVector>(reader);
            res.TangentSigns = serializer.Deserialize<PackedIntVector>(reader);
            if (version.GreaterThanOrEquals(5))
            {
                res.FloatColors = serializer.Deserialize<PackedFloatVector>(reader);
            }
            res.BoneIndices = serializer.Deserialize<PackedIntVector>(reader);
            res.Triangles = serializer.Deserialize<PackedIntVector>(reader);
            if (version.GreaterThanOrEquals(3, 5)) //3.5 and up
            {
                if (version.LessThan(5))
                {
                    res.Colors = serializer.Deserialize<PackedIntVector>(reader);
                }
                else
                {
                    res.UVInfo = reader.ReadUInt32();
                }
            }
            return res;
        }
    }

}
