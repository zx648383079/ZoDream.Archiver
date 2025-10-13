using System;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal sealed class TBin10Converter : BundleConverter<TBin10>
    {
        public override TBin10? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new TBin10();
            res.Format = reader.ReadString(6);
            res.Id = reader.ReadString();
            res.Description = reader.ReadString();
            res.Properties = reader.ReadArray(_ => serializer.Deserialize<Property>(reader));
            res.TileSheets = reader.ReadArray(_ => serializer.Deserialize<TileSheet>(reader));
            var isRemoveTileSheetsExtension = false;
            if (!isRemoveTileSheetsExtension)
            {
                res.Layers = reader.ReadArray(_ => serializer.Deserialize<Layer>(reader));
            }
            return res;
        }
    }
}
