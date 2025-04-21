using System;
using System.Numerics;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class HumanPoseConverter : BundleConverter<HumanPose>
    {
        public override HumanPose? Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var res = new HumanPose();
            var version = reader.Get<Version>();
            res.RootX = reader.ReadXForm();
            res.LookAtPosition = version.GreaterThanOrEquals(5, 4) ? reader.ReadVector3Or4() :
                reader.ReadVector4().AsVector3();//5.4 and up
            res.LookAtWeight = reader.ReadVector4();

            res.GoalArray = reader.ReadArray<HumanGoal>(serializer);

            res.LeftHandPose = serializer.Deserialize<HandPose>(reader);
            res.RightHandPose = serializer.Deserialize<HandPose>(reader);

            res.DoFArray = reader.ReadArray(r => r.ReadSingle());

            if (version.GreaterThanOrEquals(5, 2))//5.2 and up
            {
                res.TDoFArray = reader.ReadVector3Array();
            }
            return res;
        }

    }

}
