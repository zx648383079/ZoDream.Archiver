namespace ZoDream.KhronosExporter.Models
{
    public class MaterialsClearCoat
    {
        public const string ExtensionName = "KHR_materials_clearcoat";

        public float ClearcoatFactor { get; set; }

        public TextureInfo? ClearcoatTexture { get; set; }

        public float ClearcoatRoughnessFactor { get; set; }

        public TextureInfo? ClearcoatRoughnessTexture { get; set; }

        public MaterialNormalTextureInfo? ClearcoatNormalTexture { get; set; }
    }
}
