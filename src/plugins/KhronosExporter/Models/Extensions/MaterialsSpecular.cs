using System.ComponentModel.DataAnnotations;

namespace ZoDream.KhronosExporter.Models
{
    public class MaterialsSpecular
    {
        public const string ExtensionName = "KHR_materials_specular";

        public float SpecularFactor { get; set; } = 1f;

        public TextureInfo? SpecularTexture { get; set; }

        [Length(3, 3)]
        public float[]? SpecularColorFactor { get; set; }

        public TextureInfo? SpecularColorTexture { get; set; }
    }
}
