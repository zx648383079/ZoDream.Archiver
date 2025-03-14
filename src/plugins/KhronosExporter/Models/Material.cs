namespace ZoDream.KhronosExporter.Models
{
    public class Material : LogicalChildOfRoot
    {


        public MaterialPbrMetallicRoughness PbrMetallicRoughness { get; set; }
        /// <summary>
        /// 法线贴图
        /// </summary>
        public MaterialNormalTextureInfo NormalTexture {  get; set; }
        /// <summary>
        /// 遮挡贴图
        /// </summary>
        public MaterialOcclusionTextureInfo OcclusionTexture {  get; set; }
        /// <summary>
        /// 自发光贴图
        /// </summary>
        public TextureInfo EmissiveTexture {  get; set; }
        /// <summary>
        /// 自发光颜色
        /// </summary>
        public float[] EmissiveFactor {  get; set; }

        public AlphaMode AlphaMode {  get; set; }

        /// <summary>
        /// AlphaMode.MASK 使用
        /// </summary>
        public float? AlphaCutoff {  get; set; }

        public bool DoubleSided {  get; set; }
    }
}
