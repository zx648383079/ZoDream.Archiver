using System;
using System.IO;

namespace ZoDream.Shared.Bundle.Converters
{
    public class StreamConverter : BundleConverter<Stream>
    {
        public override Stream Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return reader.ReadAsStream();
        }
    }
}
