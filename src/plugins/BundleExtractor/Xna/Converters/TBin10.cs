using System;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal sealed class TBin10Converter : BundleConverter<TBin10>
    {
        public override TBin10? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new TBin10
            {
                Format = reader.ReadString(6),
                Id = XnbReader.ReadString(reader),
                Description = XnbReader.ReadString(reader),
                Properties = reader.ReadArray(_ => serializer.Deserialize<Property>(reader)),
                TileSheets = reader.ReadArray(_ => serializer.Deserialize<TileSheet>(reader))
            };
            var isRemoveTileSheetsExtension = false;
            if (!isRemoveTileSheetsExtension)
            {
                res.Layers = reader.ReadArray(_ => serializer.Deserialize<Layer>(reader));
            }
            return res;
        }
    }
}
