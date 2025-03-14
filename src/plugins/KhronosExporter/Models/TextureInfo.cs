namespace ZoDream.KhronosExporter.Models
{
    public class TextureInfo : ExtraProperties
    {
        /// <summary>
        /// 指向 textures[]
        /// </summary>
        public int? Index { get; set; }
        /// <summary>
        /// 指向 mesh->Primitive->Attributes->TEXCOORD_{int}
        /// </summary>
        public int? TexCoord { get; set; }
    }
}
