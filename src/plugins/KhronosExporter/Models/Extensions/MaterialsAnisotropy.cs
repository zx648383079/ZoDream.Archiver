namespace ZoDream.KhronosExporter.Models
{
    /// <summary>
    /// 此扩展将材质的各向异性属性定义为可观察到的拉丝金属。 引入了不对称镜面反射叶模型来允许此类现象。 该波瓣的视觉独特特征是镜面反射的拉长外观。
    /// </summary>
    public class MaterialsAnisotropy
    {
        public const string ExtensionName = "KHR_materials_anisotropy";

        public float AnisotropyStrength { get; set; }
        public float AnisotropyRotation { get; set; }
        public TextureInfo? AnisotropyTexture { get; set; }
    }
}
