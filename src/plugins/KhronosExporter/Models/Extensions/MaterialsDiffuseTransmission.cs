using System.Numerics;

namespace ZoDream.KhronosExporter.Models
{
    /// <summary>
    /// 模拟了光通过无限薄的材料漫射传输的物理现象
    /// </summary>
    public class MaterialsDiffuseTransmission
    {
        public const string ExtensionName = "KHR_materials_diffuse_transmission";

        public float DiffuseTransmissionFactor { get; set; }

        public TextureInfo? DiffuseTransmissionTexture { get; set; }

        public Vector3? DiffuseTransmissionColorFactor { get; set; }

        public TextureInfo? DiffuseTransmissionColorTexture { get; set; }
    }
}
