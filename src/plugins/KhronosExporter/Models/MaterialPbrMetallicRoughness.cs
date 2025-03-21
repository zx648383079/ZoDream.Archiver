using System.Numerics;

namespace ZoDream.KhronosExporter.Models
{
    /// <summary>
    /// 可以搭配 KHR_materials_specular
    /// </summary>
    public class MaterialPbrMetallicRoughness : ExtraProperties
    {
        public Vector4? BaseColorFactor { get; set; }
        /// <summary>
        /// 贴图和颜色只能有一个
        /// </summary>
        public TextureInfo? BaseColorTexture { get; set; }

        /// <summary>
        /// 0-1 金属程度 越反光
        /// </summary>
        public float MetallicFactor { get; set; }

        /// <summary>
        /// 0-1 粗糙度 越不反光
        /// </summary>
        public float RoughnessFactor { get; set; }

        public TextureInfo? MetallicRoughnessTexture { get; set; }
    }
}