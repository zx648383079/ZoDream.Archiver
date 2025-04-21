using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class AnimationClipBindingConstantConverter : BundleConverter<AnimationClipBindingConstant>
    {
        public override AnimationClipBindingConstant? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            return new()
            {
                GenericBindings = reader.ReadArray<GenericBinding>(serializer),
                CurveMapping = reader.ReadArray<PPtr>(serializer)
            };
        }

        public GenericBinding FindBinding(AnimationClipBindingConstant res, int index)
        {
            int curves = 0;
            foreach (var b in res.GenericBindings)
            {
                if (b.TypeID == NativeClassID.Transform)
                {
                    curves += b.Attribute switch
                    {
                        //kBindTransformPosition
                        1 or 3 or 4 => 3,
                        //kBindTransformRotation
                        2 => 4,
                        _ => 1,
                    };
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
