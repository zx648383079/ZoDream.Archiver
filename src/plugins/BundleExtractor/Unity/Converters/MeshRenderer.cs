using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal sealed class MeshRendererConverter : BundleConverter<MeshRenderer>
    {
        public override MeshRenderer? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new MeshRenderer();

            RendererConverter.Read(res, reader, serializer);

            res.AdditionalVertexStreams = reader.ReadPPtr<Mesh>(serializer);
            return res;
        }
    }
}
