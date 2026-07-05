using System;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class YarnConverter : BundleConverter<YarnProject>
    {
        public override YarnProject? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var dataLength = reader.ReadInt32();
            reader.Position += dataLength;
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                XnbReader.ReadString(reader);
                XnbReader.ReadString(reader);
                reader.ReadArray(reader.ReadInt32(), _ => XnbReader.ReadString(reader));
            }
            count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                XnbReader.ReadString(reader);
                XnbReader.ReadString(reader);
                XnbReader.ReadString(reader);
            }
            return new YarnProject();
        }
    }

    internal class CdbConverter : BundleConverter<Cdb>
    {
        public override Cdb? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                var text = XnbReader.ReadString(reader);
                var name = XnbReader.ReadString(reader);
                var dataLength = reader.ReadInt32();
                reader.Position += dataLength;
            }
            return new();
        }
    }
}
