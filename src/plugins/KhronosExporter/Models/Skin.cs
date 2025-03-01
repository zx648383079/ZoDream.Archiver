namespace ZoDream.KhronosExporter.Models
{
    public class Skin : LogicalChildOfRoot
    {
        /// <summary>
        /// Accessor
        /// </summary>
        public int InverseBindMatrices { get; set; }

        public int Skeleton {  get; set; }
        /// <summary>
        /// Nodes
        /// </summary>
        public int[] Joints { get; set; }

    }
}
