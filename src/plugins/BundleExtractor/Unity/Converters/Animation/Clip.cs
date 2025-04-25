using System;
using System.Collections.Generic;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class ClipConverter : BundleConverter<Clip>
    {
        public override Clip? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new Clip
            {
                StreamedClip = serializer.Deserialize<StreamedClip>(reader),
                DenseClip = serializer.Deserialize<DenseClip>(reader)
            };
            if (version.GreaterThanOrEquals(4, 3)) //4.3 and up
            {
                res.ConstantClip = serializer.Deserialize<ConstantClip>(reader);
            }
            if (version.LessThan(2018, 3)) //2018.3 down
            {
                res.Binding = serializer.Deserialize<ValueArrayConstant>(reader);
            }
            return res;
        }

        public static AnimationClipBindingConstant ConvertValueArrayToGenericBinding(Clip res)
        {
            var bindings = new AnimationClipBindingConstant();
            var genericBindings = new List<GenericBinding>();
            var values = res.Binding;
            for (int i = 0; i < values.ValueArray.Length;)
            {
                var curveID = values.ValueArray[i].ID;
                var curveTypeID = values.ValueArray[i].TypeID;
                var binding = new GenericBinding();
                genericBindings.Add(binding);
                if (curveTypeID == 4174552735) //CRC(PositionX))
                {
                    binding.Path = curveID;
                    binding.Attribute = 1; //kBindTransformPosition
                    binding.TypeID = NativeClassID.Transform;
                    i += 3;
                }
                else if (curveTypeID == 2211994246) //CRC(QuaternionX))
                {
                    binding.Path = curveID;
                    binding.Attribute = 2; //kBindTransformRotation
                    binding.TypeID = NativeClassID.Transform;
                    i += 4;
                }
                else if (curveTypeID == 1512518241) //CRC(ScaleX))
                {
                    binding.Path = curveID;
                    binding.Attribute = 3; //kBindTransformScale
                    binding.TypeID = NativeClassID.Transform;
                    i += 3;
                }
                else
                {
                    binding.TypeID = NativeClassID.Animator;
                    binding.Path = 0;
                    binding.Attribute = curveID;
                    i++;
                }
            }
            bindings.GenericBindings = [..genericBindings];
            return bindings;
        }
    }
}
