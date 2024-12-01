using System.Numerics;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class QuaternionCurve : IYamlWriter
    {
        public AnimationCurve<Vector4> curve;
        public string path;

        public QuaternionCurve(string path)
        {
            curve = new AnimationCurve<Vector4>();
            this.path = path;
        }

        public QuaternionCurve(IBundleBinaryReader reader)
        {
            curve = new AnimationCurve<Vector4>(reader, reader.ReadVector4);
            path = reader.ReadAlignedString();
        }

        //public YAMLNode ExportYAML(UnityVersion version)
        //{
        //    YAMLMappingNode node = new YAMLMappingNode();
        //    node.Add(nameof(curve), curve.ExportYAML(version));
        //    node.Add(nameof(path), path);
        //    return node;
        //}
        public override bool Equals(object obj)
        {
            if (obj is QuaternionCurve quaternionCurve)
            {
                return path == quaternionCurve.path;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 199;
            unchecked
            {
                hash = 617 + hash * path.GetHashCode();
            }
            return hash;
        }
    }

}
