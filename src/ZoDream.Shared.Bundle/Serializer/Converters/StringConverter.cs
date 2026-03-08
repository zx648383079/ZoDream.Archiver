using System;

namespace ZoDream.Shared.Bundle.Converters
{
    public class BooleanConverter : BundleConverter<bool>
    {
        public override bool Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadBoolean();
        }
    }
    public class CharConverter : BundleConverter<char>
    {
        public override char Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadChar();
        }
    }

    public class StringConverter : BundleConverter<string>
    {
        public override string Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.Read7BitEncodedString();
        }
    }
}
