using System;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class TileSheetConverter : BundleConverter<TileSheet>
    {
        public override TileSheet? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new TileSheet()
            {
                Id = reader.ReadString(),
                Description = reader.ReadString(),
                Image = reader.ReadString(),
                SheetSize = XnbReader.ReadVector2I(reader),
                TileSize = XnbReader.ReadVector2I(reader),
                Margin = XnbReader.ReadVector2I(reader),
                Spacing = XnbReader.ReadVector2I(reader),
                Properties = reader.ReadArray(_ => serializer.Deserialize<Property>(reader)),
            };
            
            return res;
        }
    }
}
