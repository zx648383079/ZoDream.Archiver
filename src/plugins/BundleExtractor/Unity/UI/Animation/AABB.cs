using System.Numerics;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class AABB : IYamlWriter
    {
        public Vector3 m_Center;
        public Vector3 m_Extent;

        public AABB(UIReader reader)
        {
            m_Center = reader.ReadVector3();
            m_Extent = reader.ReadVector3();
        }

        //public YAMLNode ExportYAML(UnityVersion version)
        //{
        //    var node = new YAMLMappingNode();
        //    node.Add(nameof(m_Center), m_Center.ExportYAML(version));
        //    node.Add(nameof(m_Extent), m_Extent.ExportYAML(version));
        //    return node;
        //}
    }

}
