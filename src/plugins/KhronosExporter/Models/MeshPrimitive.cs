using System.Collections.Generic;

namespace ZoDream.KhronosExporter.Models
{
    public class MeshPrimitive : ExtraProperties
    {
        /// <summary>
        /// JOINTS_0：存储了骨骼节点,顶点受到哪些骨骼节点的制约
        /// NORMAL ： 存储了法线,顶点的法线
        /// POSITION：存储了位置信息,顶点的具体位置
        /// TANGENT：存储了切线,顶点的切线
        /// TEXCOORD_0: 存储了纹理, 顶点的uv坐标
        /// WEIGHTS_0：权重数据, 顶点受到骨骼节点制约的权重，这个权重和JOINTS_0数组里的骨骼节点是一一对应的
        /// </summary>
        public Dictionary<string, int> Attributes { get; set; } = [];
        public int Indices { get; set; }

        public int Material {  get; set; }

        public PrimitiveType Mode { get; set; } = PrimitiveType.TRIANGLES;

        public IList<Dictionary<string, int>> Targets { get; set; } = [];
    }
}
