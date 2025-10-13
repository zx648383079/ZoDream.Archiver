using System;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Numerics;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class PointConverter : BundleConverter<Vector2Int>
    {
        public override Vector2Int Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return XnbReader.ReadVector2I(reader);
        }
    }
}
