using System;

namespace ZoDream.Shared.Bundle.Converters
{
    public class DecimalConverter : BundleConverter<decimal>
    {
        public override decimal Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadDecimal();
        }
    }

    public class DoubleConverter : BundleConverter<double>
    {
        public override double Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadDouble();
        }
    }

    public class SingleConverter : BundleConverter<float>
    {
        public override float Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadSingle();
        }
    }
}
