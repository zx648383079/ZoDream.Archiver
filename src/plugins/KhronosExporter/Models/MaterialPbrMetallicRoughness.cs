namespace ZoDream.KhronosExporter.Models
{
    public class MaterialPbrMetallicRoughness : ExtraProperties
    {
        public float[] BaseColorFactor { get; set; }

        public TextureInfo BaseColorTexture { get; set; }

        /// <summary>
        /// 0-1 金属程度 越反光
        /// </summary>
        public float MetallicFactor { get; set; }

        /// <summary>
        /// 0-1 粗糙度 越不反光
        /// </summary>
        public float RoughnessFactor { get; set; }

        public TextureInfo MetallicRoughnessTexture { get; set; }
    }
}