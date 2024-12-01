using System.Numerics;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SpriteVertex
    {
        public Vector3 pos;
        public Vector2 uv;

        public SpriteVertex(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();

            pos = reader.ReadVector3();
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and down
            {
                uv = reader.ReadVector2();
            }
        }
    }

}
