using System.ComponentModel.DataAnnotations;

namespace ZoDream.KhronosExporter.Models
{
    public class MaterialPBRSpecularGlossiness
    {
        public const string ExtensionName = "KHR_materials_pbrSpecularGlossiness";

        [Length(4, 4)]
        public float[]? DiffuseFactor { get; set; }

        public TextureInfo? DiffuseTexture { get; set; }

        [Length(3,3)]
        public float[]? SpecularFactor { get; set; }

        public float GlossinessFactor { get; set; } = 1f;

        public TextureInfo? SpecularGlossinessTexture { get; set; }
    }
}
