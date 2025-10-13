using System;
using System.Collections.Generic;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class AnimatedTileConverter : BundleConverter<AnimatedTile>
    {
        public override AnimatedTile? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new AnimatedTile();
            res.FrameInterval = reader.ReadInt32();
            var frameCount = reader.ReadInt32();
            var tileSheet = new List<string>()
            {
                string.Empty
            };
            var indexItems = new List<char>();
            var frameItems = new List<StaticTile>();
            var sheetIndex = 0;
            var frameIndex = 0;
            while (frameIndex < frameCount)
            {
                var code = (char)reader.ReadByte();
                indexItems.Add(code);
                switch (code)
                {
                    case 'T':
                        tileSheet.Add(reader.ReadString());
                        sheetIndex++;
                        break;
                    case 'S':
                        var tile = serializer.Deserialize<StaticTile>(reader);
                        tile.TileSheet = tileSheet[sheetIndex];
                        frameItems.Add(tile);
                        frameIndex++;
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
            res.Frames = frameItems.ToArray();
            res.Index = indexItems.ToArray();
            res.Properties = reader.ReadArray(_ => serializer.Deserialize<Property>(reader));
            return res;
        }
    }
}
