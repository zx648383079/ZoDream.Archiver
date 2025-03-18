using System.Numerics;

namespace ZoDream.KhronosExporter.Models
{
    /// <summary>
    /// 控制高光反射
    /// </summary>
    public class MaterialsSpecular
    {
        public const string ExtensionName = "KHR_materials_specular";

        public float SpecularFactor { get; set; } = 1f;

        public TextureInfo? SpecularTexture { get; set; }

        public Vector3? SpecularColorFactor { get; set; }

        public TextureInfo? SpecularColorTexture { get; set; }
    }
}
