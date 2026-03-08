using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class CharConverter : BundleConverter<char>
    {
        public override char Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return XnbReader.ReadChar(reader);
        }
    }


    internal class BoundingBoxConverter : BundleConverter<MinMaxAABB>
    {
        public override MinMaxAABB Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new MinMaxAABB()
            {
                Min = reader.ReadVector3(),
                Max = reader.ReadVector3(),
            };
        }
    }


    internal class DateTimeConverter : BundleConverter<DateTime>
    {
        public override DateTime Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var num = reader.ReadUInt64();
            var num2 = 13835058055282163712uL;
            var ticks = (long)(num & ~num2);
            var kind = (DateTimeKind)((num >> 62) & 3);
            return new DateTime(ticks, kind);
        }
    }

    

}
