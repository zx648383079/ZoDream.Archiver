using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class MeshBlendShapeConverter : BundleConverter<MeshBlendShape>
    {
        public void ReadBase(ref MeshBlendShape res, IBundleBinaryReader reader, 
            IBundleSerializer serializer, Action cb)
        {
            var version = reader.Get<Version>();

            if (version.LessThan(4, 3)) //4.3 down
            {
                res.Name = reader.ReadAlignedString();
            }
            res.FirstVertex = reader.ReadUInt32();
            res.VertexCount = reader.ReadUInt32();
            if (version.LessThan(4, 3)) //4.3 down
            {
                var aabbMinDelta = reader.ReadVector3Or4();
                var aabbMaxDelta = reader.ReadVector3Or4();
            }
            res.HasNormals = reader.ReadBoolean();
            res.HasTangents = reader.ReadBoolean();
        }

        public override MeshBlendShape Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new MeshBlendShape();
            ReadBase(ref res, reader, serializer, () => { });
            var version = reader.Get<Version>();
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                reader.AlignStream();
            }
            return res;
        }
    }

}
