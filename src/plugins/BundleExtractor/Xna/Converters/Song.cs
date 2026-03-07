using System;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class SongConverter : BundleConverter<Song>
    {
        public override Song? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new Song()
            {
                FileName = XnbReader.ReadString(reader),
                Duration = reader.ReadInt32(), // ms
            };
        }
    }
}
