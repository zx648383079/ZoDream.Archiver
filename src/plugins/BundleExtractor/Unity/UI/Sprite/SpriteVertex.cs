using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class SpriteVertex
    {
        public Vector3 pos;
        public Vector2 uv;

        public SpriteVertex(UIReader reader)
        {
            var version = reader.Version;

            pos = reader.ReadVector3();
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and down
            {
                uv = reader.ReadVector2();
            }
        }
    }

}
