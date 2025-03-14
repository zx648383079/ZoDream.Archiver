namespace ZoDream.KhronosExporter.Models
{
    public class Skin : LogicalChildOfRoot
    {
        /// <summary>
        /// 引用一个 accessor，存储每个关节的逆绑定矩阵（inverse bind matrix）。这些矩阵用于将顶点从绑定姿势转换到关节的局部空间。
        /// </summary>
        public int InverseBindMatrices { get; set; }

        public int Skeleton {  get; set; }
        /// <summary>
        /// 包含所有关节（骨骼）的节点索引。这些索引指向 nodes 数组中的节点。
        /// </summary>
        public int[] Joints { get; set; }

    }
}
