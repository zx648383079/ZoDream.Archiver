using System;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class StaticTileConverter : BundleConverter<StaticTile>
    {
        public override StaticTile? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new StaticTile()
            {
                TileIndex = reader.ReadInt32(),
                BlendMode = reader.ReadByte(),
                Properties = reader.ReadArray(_ => serializer.Deserialize<Property>(reader)),
            };
        }
    }
}
