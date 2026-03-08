using System;

namespace ZoDream.Shared.Bundle.Converters
{
    public class ByteConverter : BundleConverter<byte>
    {
        public override byte Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadByte();
        }
    }

    public class UInt16Converter : BundleConverter<ushort>
    {
        public override ushort Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadUInt16();
        }
    }
    public class UInt32Converter : BundleConverter<uint>
    {
        public override uint Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadUInt32();
        }
    }

    public class UInt64Converter : BundleConverter<ulong>
    {
        public override ulong Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadUInt64();
        }
    }
}
