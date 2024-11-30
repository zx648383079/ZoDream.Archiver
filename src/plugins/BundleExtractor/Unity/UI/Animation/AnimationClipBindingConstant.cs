using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZoDream.BundleExtractor.Unity.UI
{
    internal class AnimationClipBindingConstant : IYamlWriter
    {
        public List<GenericBinding> genericBindings;
        public List<PPtr<UIObject>> pptrCurveMapping;

        public AnimationClipBindingConstant() { }

        public AnimationClipBindingConstant(UIReader reader)
        {
            int numBindings = reader.ReadInt32();
            genericBindings = new List<GenericBinding>();
            for (int i = 0; i < numBindings; i++)
            {
                genericBindings.Add(new GenericBinding(reader));
            }

            int numMappings = reader.ReadInt32();
            pptrCurveMapping = new List<PPtr<UIObject>>();
            for (int i = 0; i < numMappings; i++)
            {
                pptrCurveMapping.Add(new PPtr<UIObject>(reader));
            }
        }

        //public YAMLNode ExportYAML(int[] version)
        //{
        //    var node = new YAMLMappingNode();
        //    node.Add(nameof(genericBindings), genericBindings.ExportYAML(version));
        //    node.Add(nameof(pptrCurveMapping), pptrCurveMapping.ExportYAML(version));
        //    return node;
        //}

        public GenericBinding FindBinding(int index)
        {
            int curves = 0;
            foreach (var b in genericBindings)
            {
                if (b.typeID == ElementIDType.Transform)
                {
                    switch (b.attribute)
                    {
                        case 1: //kBindTransformPosition
                        case 3: //kBindTransformScale
                        case 4: //kBindTransformEuler
                            curves += 3;
                            break;
                        case 2: //kBindTransformRotation
                            curves += 4;
                            break;
                        default:
                            curves += 1;
                            break;
                    }
                }
                else
                {
                    curves += 1;
                }
                if (curves > index)
                {
                    return b;
                }
            }

            return null;
        }
    }
}
