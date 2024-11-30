using System;
using ZoDream.BundleExtractor.Models;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class Keyframe<T> : IYamlWriter
    {
        public float time;
        public T value;
        public T inSlope;
        public T outSlope;
        public int weightedMode;
        public T inWeight;
        public T outWeight;

        public Keyframe(float time, T value, T inSlope, T outSlope, T weight)
        {
            this.time = time;
            this.value = value;
            this.inSlope = inSlope;
            this.outSlope = outSlope;
            weightedMode = 0;
            inWeight = weight;
            outWeight = weight;
        }

        public Keyframe(UIReader reader, Func<T> readerFunc)
        {
            time = reader.ReadSingle();
            value = readerFunc();
            inSlope = readerFunc();
            outSlope = readerFunc();
            if (reader.Version.Major >= 2018) //2018 and up
            {
                weightedMode = reader.ReadInt32();
                inWeight = readerFunc();
                outWeight = readerFunc();
            }
        }

        //public YAMLNode ExportYAML(UnityVersion version)
        //{
        //    var node = new YAMLMappingNode();
        //    node.AddSerializedVersion(ToSerializedVersion(version));
        //    node.Add(nameof(time), time);
        //    node.Add(nameof(value), value.ExportYAML(version));
        //    node.Add(nameof(inSlope), inSlope.ExportYAML(version));
        //    node.Add(nameof(outSlope), outSlope.ExportYAML(version));
        //    if (version.GreaterThanOrEquals(2018)) //2018 and up
        //    {
        //        node.Add(nameof(weightedMode), weightedMode);
        //        node.Add(nameof(inWeight), inWeight.ExportYAML(version));
        //        node.Add(nameof(outWeight), outWeight.ExportYAML(version));
        //    }
        //    return node;
        //}

        private int ToSerializedVersion(UnityVersion version)
        {
            if (version.GreaterThanOrEquals(2018)) //2018 and up
            {
                return 3;
            }
            else if (version.GreaterThanOrEquals(5, 5))
            {
                return 2;
            }
            return 1;
        }
    }

}
