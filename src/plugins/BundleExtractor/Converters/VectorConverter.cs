using System;
using ZoDream.Shared.Bundle;
using ZoDream.Shared.Numerics;

namespace ZoDream.BundleExtractor.Converters
{
    internal class Vector2IConverter : BundleConverter<Vector2Int>
    {
        public override Vector2Int Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new Vector2Int(reader.ReadInt32(), reader.ReadInt32());
        }
    }

    internal class Vector3IConverter : BundleConverter<Vector3Int>
    {
        public override Vector3Int Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new Vector3Int(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        }
    }

    internal class Vector4IConverter : BundleConverter<Vector4Int>
    {
        public override Vector4Int Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new Vector4Int(reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32(), reader.ReadInt32());
        }
    }

    internal class ColorConverter : BundleConverter<Color>
    {
        public override Color Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new Color(reader.ReadByte(), reader.ReadByte(), reader.ReadByte(), reader.ReadByte());
        }
    }
}
