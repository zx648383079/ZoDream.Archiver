using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class SpriteVertexConverter : BundleConverter<SpriteVertex>
    {
        public override SpriteVertex Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new SpriteVertex();
            res.Pos = reader.ReadVector3Or4();
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and down
            {
                res.Uv = reader.ReadVector2();
            }
            return res;
        }
    }

}
