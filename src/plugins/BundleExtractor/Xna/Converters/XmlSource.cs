using System;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class XmlSourceConverter : BundleConverter<string>
    {
        public override string? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.Read7BitEncodedString();
        }
    }
}
