using System;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class VertexBufferConverter : BundleConverter<VertexBuffer>
    {
        public override VertexBuffer? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var vertexDeclaration = serializer.Deserialize<VertexDeclaration>(reader);
            var length = (int)reader.ReadUInt32() * vertexDeclaration.VertexStride;
            reader.ReadAsStream(length);
            return new VertexBuffer();
        }
    }
}
