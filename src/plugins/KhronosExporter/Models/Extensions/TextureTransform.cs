using System.Numerics;

namespace ZoDream.KhronosExporter.Models
{
    public class TextureTransform
    {
        public const string ExtensionName = "KHR_texture_transform";

        public Vector2 Offset { get; set; } = Vector2.Zero;
        public float Rotation { get; set; } = 0;
        public Vector2 Scale { get; set; } = Vector2.One;
        public int? texCoord { get; set; }
    }
}
