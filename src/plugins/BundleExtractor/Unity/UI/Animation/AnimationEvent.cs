using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class AnimationEvent : IYamlWriter
    {
        public float time;
        public string functionName;
        public string data;
        public PPtr<UIObject> objectReferenceParameter;
        public float floatParameter;
        public int intParameter;
        public int messageOptions;

        public AnimationEvent(UIReader reader)
        {
            var version = reader.Version;

            time = reader.ReadSingle();
            functionName = reader.ReadAlignedString();
            data = reader.ReadAlignedString();
            objectReferenceParameter = new PPtr<UIObject>(reader);
            floatParameter = reader.ReadSingle();
            if (version.Major >= 3) //3 and up
            {
                intParameter = reader.ReadInt32();
            }
            messageOptions = reader.ReadInt32();
        }

        //public YAMLNode ExportYAML(int[] version)
        //{
        //    var node = new YAMLMappingNode();
        //    node.Add(nameof(time), time);
        //    node.Add(nameof(functionName), functionName);
        //    node.Add(nameof(data), data);
        //    node.Add(nameof(objectReferenceParameter), objectReferenceParameter.ExportYAML(version));
        //    node.Add(nameof(floatParameter), floatParameter);
        //    node.Add(nameof(intParameter), intParameter);
        //    node.Add(nameof(messageOptions), messageOptions);
        //    return node;
        //}
    }
}
