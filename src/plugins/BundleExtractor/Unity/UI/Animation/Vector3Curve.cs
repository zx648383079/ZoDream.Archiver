using System.Numerics;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Vector3Curve : IYamlWriter
    {
        public AnimationCurve<Vector3> curve;
        public string path;

        public Vector3Curve(string path)
        {
            curve = new AnimationCurve<Vector3>();
            this.path = path;
        }

        public Vector3Curve(UIReader reader)
        {
            curve = new AnimationCurve<Vector3>(reader, reader.ReadVector3);
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
            if (obj is Vector3Curve vector3Curve)
            {
                return path == vector3Curve.path;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 577;
            unchecked
            {
                hash = 419 + hash * path.GetHashCode();
            }
            return hash;
        }
    }

}
