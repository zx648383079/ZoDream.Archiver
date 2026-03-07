using System;
using System.IO;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class TBinConverter : BundleConverter<Stream>
    {
        public override Stream? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            reader.ReadAsStream();
            return null;
        }
    }
}
