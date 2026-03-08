using System;

namespace ZoDream.Shared.Bundle.Converters
{
    public class Int16Converter : BundleConverter<short>
    {
        public override short Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadInt16();
        }
    }

    public class Int32Converter : BundleConverter<int>
    {
        public override int Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadInt32();
        }
    }

    public class Int64Converter : BundleConverter<long>
    {
        public override long Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadInt64();
        }
    }

    public class SByteConverter : BundleConverter<sbyte>
    {
        public override sbyte Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadSByte();
        }
    }
}
