namespace ZoDream.KhronosExporter.Models
{
    public class Texture : LogicalChildOfRoot
    {
        /// <summary>
        /// 指向 TextureSampler
        /// </summary>
        public int? Sampler { get; set; }

        /// <summary>
        /// 指向 image
        /// </summary>
        public int? Source { get; set; }

    }
}
