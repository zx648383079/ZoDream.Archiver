using System;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class PropertyConverter : BundleConverter<Property>
    {
        public override Property? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new Property()
            {
                Key = reader.ReadString(),
                Type = reader.ReadByte()
            };
            res.Value = res.Type switch
            {
                0 => reader.ReadByte(),
                1 => reader.ReadInt32(),
                2 => reader.ReadSingle(),
                3 => reader.ReadString(),
                _ => throw new NotSupportedException()
            };

            return res;
        }
    }
}
