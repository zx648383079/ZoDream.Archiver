namespace ZoDream.KhronosExporter.Models
{
    public class MaterialPbrMetallicRoughness : ExtraProperties
    {
        public float[] BaseColorFactor { get; set; }

        public TextureInfo BaseColorTexture { get; set; }

        public float MetallicFactor { get; set; }

        public float RoughnessFactor { get; set; }

        public TextureInfo MetallicRoughnessTexture { get; set; }
    }
}