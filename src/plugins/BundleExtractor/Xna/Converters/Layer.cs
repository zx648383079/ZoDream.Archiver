using System;
using System.Collections.Generic;
using System.Linq;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class LayerConverter : BundleConverter<Layer>
    {
        public override Layer? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new Layer();
            res.Id = reader.ReadString();
            res.Visible = reader.ReadByte();
            res.Description = reader.ReadString();
            res.LayerSize = XnbReader.ReadVector2I(reader);
            res.TileSize = XnbReader.ReadVector2I(reader);
            res.Properties = reader.ReadArray(_ => serializer.Deserialize<Property>(reader));
            float num = res.LayerSize.X;
            float num2 = res.LayerSize.Y;
            var tileSheet = new List<string>() { string.Empty };
            var tiles = new List<BaseTile>((int)(num * num2));
            var indexItems = new List<char>();
            var sizeItems = new List<int>();
            for (int i = 0; i < num2; i++)
            {
                int num3 = 0;
                while (num3 < num)
                {
                    var c = (char)reader.ReadByte();
                    indexItems.Add(c);
                    switch (c)
                    {
                        case 'N':
                            {
                                int num4 = reader.ReadInt32();
                                sizeItems.Add(num4);
                                for (int j = 0; j < num4; j++)
                                {
                                    tiles.Add(null);
                                }
                                num3 += num4;
                                break;
                            }
                        case 'S':
                            {
                                var staticTile = serializer.Deserialize<StaticTile>(reader);
                                staticTile.TileSheet = tileSheet.Last();
                                tiles.Add(staticTile);
                                num3++;
                                break;
                            }
                        case 'A':
                            tiles.Add(serializer.Deserialize<AnimatedTile>(reader));
                            num3++;
                            break;
                        case 'T':
                            tileSheet.Add(reader.ReadString());
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                }
            }
            return res;
        }
    }
}
