using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SubMeshConverter : BundleConverter<SubMesh>
    {
        public override SubMesh Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new SubMesh
            {
                FirstByte = reader.ReadUInt32(),
                IndexCount = reader.ReadUInt32(),
                Topology = (GfxPrimitiveType)reader.ReadInt32()
            };

            if (version.LessThan(4)) //4.0 down
            {
                res.TriangleCount = reader.ReadUInt32();
            }

            if (version.GreaterThanOrEquals(2017, 3)) //2017.3 and up
            {
                res.BaseVertex = reader.ReadUInt32();
            }

            if (version.GreaterThanOrEquals(3)) //3.0 and up
            {
                res.FirstVertex = reader.ReadUInt32();
                res.VertexCount = reader.ReadUInt32();
                res.LocalAABB = serializer.Deserialize<Bounds>(reader);
            }
            return res;
        }
    }

}
