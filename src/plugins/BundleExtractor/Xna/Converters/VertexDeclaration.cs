using System;
using ZoDream.BundleExtractor.Xna.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Xna.Converters
{
    internal class VertexDeclarationConverter : BundleConverter<VertexDeclaration>
    {
        public override VertexDeclaration? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var vertexStride = reader.ReadInt32();
            reader.ReadArray<object>(_ => {
                var offset = reader.ReadInt32();
                var elementFormat = reader.ReadInt32();
                var elementUsage = reader.ReadInt32();
                int usageIndex = reader.ReadInt32();
                return null;
            });
            return new VertexDeclaration()
            {
                VertexStride = vertexStride,
            };
        }
    }
}
