using System;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class TBinConverter : BundleConverter<object>
    {
        public override object? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            reader.ReadAsStream();
            return null;
        }
    }
}
