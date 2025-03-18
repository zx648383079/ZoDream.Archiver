using System.Numerics;

namespace ZoDream.KhronosExporter.Models
{
    public class MaterialPBRSpecularGlossiness
    {
        public const string ExtensionName = "KHR_materials_pbrSpecularGlossiness";
        /// <summary>
        /// 扩散因子
        /// </summary>
        public Vector4? DiffuseFactor { get; set; }
        /// <summary>
        /// 漫反射纹理
        /// </summary>
        public TextureInfo? DiffuseTexture { get; set; }
        /// <summary>
        /// 镜面反射因子
        /// </summary>
        public Vector3? SpecularFactor { get; set; }
        /// <summary>
        /// 光泽度因子
        /// </summary>
        public float GlossinessFactor { get; set; } = 1f;
        /// <summary>
        /// 镜面光泽度贴图
        /// </summary>
        public TextureInfo? SpecularGlossinessTexture { get; set; }
    }
}
