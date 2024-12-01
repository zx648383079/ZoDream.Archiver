using System.Collections.Generic;
using ZoDream.BundleExtractor.Models;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class PPtrCurve : IYamlWriter
    {
        public List<PPtrKeyframe> curve;
        public string attribute;
        public string path;
        public int classID;
        public PPtr<MonoScript> script;
        public int flags;

        public PPtrCurve(string path, string attribute, ElementIDType classID, PPtr<MonoScript> script)
        {
            curve = new List<PPtrKeyframe>();
            this.attribute = attribute;
            this.path = path;
            this.classID = (int)classID;
            this.script = script;
            flags = 0;
        }

        public PPtrCurve(IBundleBinaryReader reader)
        {
            var version = reader.Get<UnityVersion>();

            int numCurves = reader.ReadInt32();
            curve = new List<PPtrKeyframe>();
            for (int i = 0; i < numCurves; i++)
            {
                curve.Add(new PPtrKeyframe(reader));
            }

            attribute = reader.ReadAlignedString();
            path = reader.ReadAlignedString();
            classID = reader.ReadInt32();
            script = new PPtr<MonoScript>(reader);
            if (version.Major == 2022 && version.Minor >= 2) //2022.2 and up
            {
                flags = reader.ReadInt32();
            }
        }

        //public YAMLNode ExportYAML(UnityVersion version)
        //{
        //    YAMLMappingNode node = new YAMLMappingNode();
        //    node.Add(nameof(curve), curve.ExportYAML(version));
        //    node.Add(nameof(attribute), attribute);
        //    node.Add(nameof(path), path);
        //    node.Add(nameof(classID), (classID).ToString());
        //    node.Add(nameof(script), script.ExportYAML(version));
        //    node.Add(nameof(flags), flags);
        //    return node;
        //}

        public override bool Equals(object obj)
        {
            if (obj is PPtrCurve pptrCurve)
            {
                return this == pptrCurve;
            }
            return false;
        }

        public override int GetHashCode()
        {
            int hash = 113;
            unchecked
            {
                hash = hash + 457 * attribute.GetHashCode();
                hash = hash * 433 + path.GetHashCode();
                hash = hash * 223 + classID.GetHashCode();
                hash = hash * 911 + script.GetHashCode();
                hash = hash * 342 + flags.GetHashCode();
            }
            return hash;
        }
    }

}
