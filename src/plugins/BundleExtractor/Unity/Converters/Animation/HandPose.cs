using System;
using UnityEngine;
using ZoDream.Shared.Bundle;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class HandPoseConverter : BundleConverter<HandPose>
    {
        public override HandPose? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new HandPose
            {
                GrabX = reader.ReadXForm(),
                DoFArray = reader.ReadArray(r => r.ReadSingle()),
                Override = reader.ReadSingle(),
                CloseOpen = reader.ReadSingle(),
                InOut = reader.ReadSingle(),
                Grab = reader.ReadSingle()
            };
            return res;
        }

    }

}
