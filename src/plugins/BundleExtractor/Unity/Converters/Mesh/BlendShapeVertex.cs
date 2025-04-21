using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class BlendShapeVertexConverter : BundleConverter<BlendShapeVertex>
    {
        public override BlendShapeVertex Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new BlendShapeVertex
            {
                Vertex = reader.ReadVector3Or4(),
                Normal = reader.ReadVector3Or4(),
                Tangent = reader.ReadVector3Or4(),
                Index = reader.ReadUInt32()
            };
            return res;
        }
    }

}
