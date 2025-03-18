using System.Numerics;

namespace ZoDream.KhronosExporter.Models
{
    /// <summary>
    /// 用来控制折射率
    /// </summary>
    public class MaterialsVolume
    {
        public const string ExtensionName = "KHR_materials_volume";

        public float ThicknessFactor { get; set; }

        public TextureInfo? ThicknessTexture { get; set; }

        public float? AttenuationDistance { get; set; }

        public Vector3? AttenuationColor { get; set; }
    }
}
