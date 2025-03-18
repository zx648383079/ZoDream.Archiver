using System.Numerics;

namespace ZoDream.KhronosExporter.Models
{
    public class MaterialsSheen
    {
        public const string ExtensionName = "KHR_materials_sheen";

        public Vector3? SheenColorFactor { get; set; }

        public TextureInfo? SheenColorTexture { get; set; }

        public float SheenRoughnessFactor { get; set; } = 0;

        public TextureInfo? SheenRoughnessTexture { get; set; }
    }
}
