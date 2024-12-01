
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class CompressedAnimationCurve : IYamlWriter
    {
        public string m_Path;
        public PackedIntVector m_Times;
        public PackedQuatVector m_Values;
        public PackedFloatVector m_Slopes;
        public int m_PreInfinity;
        public int m_PostInfinity;

        public CompressedAnimationCurve(IBundleBinaryReader reader)
        {
            m_Path = reader.ReadAlignedString();
            m_Times = new PackedIntVector(reader);
            m_Values = new PackedQuatVector(reader);
            m_Slopes = new PackedFloatVector(reader);
            m_PreInfinity = reader.ReadInt32();
            m_PostInfinity = reader.ReadInt32();
        }

        //public YAMLNode ExportYAML(UnityVersion version)
        //{
        //    var node = new YAMLMappingNode();
        //    node.Add(nameof(m_Path), m_Path);
        //    node.Add(nameof(m_Times), m_Times.ExportYAML(version));
        //    node.Add(nameof(m_Values), m_Values.ExportYAML(version));
        //    node.Add(nameof(m_Slopes), m_Slopes.ExportYAML(version));
        //    node.Add(nameof(m_PreInfinity), m_PreInfinity);
        //    node.Add(nameof(m_PostInfinity), m_PostInfinity);
        //    return node;
        //}
    }

}
