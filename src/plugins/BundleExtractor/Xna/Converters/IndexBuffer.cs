using System;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class IndexBufferConverter : BundleConverter<IndexBuffer>
    {
        public override IndexBuffer? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new IndexBuffer()
            {
                Flag = reader.ReadBoolean(),
                Data = reader.ReadAsStream()
            };
        }
    }
}
