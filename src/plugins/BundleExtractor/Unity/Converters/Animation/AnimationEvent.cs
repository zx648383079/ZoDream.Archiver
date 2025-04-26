using System;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Object = UnityEngine.Object;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class AnimationEventConverter : BundleConverter<AnimationEvent>
    {
        public override AnimationEvent? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new AnimationEvent();
            res.Time = reader.ReadSingle();
            res.FunctionName = reader.ReadAlignedString();
            res.Data = reader.ReadAlignedString();
            res.ObjectReferenceParameter = reader.ReadPPtr<Object>(serializer);
            res.FloatParameter = reader.ReadSingle();
            if (version.Major >= 3) //3 and up
            {
                res.IntParameter = reader.ReadInt32();
            }
            res.MessageOptions = reader.ReadInt32();
            return res;
        }

    }
}
