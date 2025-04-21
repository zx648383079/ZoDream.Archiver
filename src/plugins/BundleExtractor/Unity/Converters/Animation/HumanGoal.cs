using System;
using System.Numerics;
using UnityEngine;
using ZoDream.Shared.Bundle;
using Version = UnityEngine.Version;

namespace ZoDream.BundleExtractor.Unity.Converters
{
    internal class HumanGoalConverter : BundleConverter<HumanGoal>
    {
        public override HumanGoal Read(IBundleBinaryReader reader, Type objectType, IBundleSerializer serializer)
        {
            var version = reader.Get<Version>();
            var res = new HumanGoal
            {
                X = reader.ReadXForm(),
                WeightT = reader.ReadSingle(),
                WeightR = reader.ReadSingle()
            };
            if (version.Major >= 5)//5.0 and up
            {
                res.HintT = version.GreaterThanOrEquals(5, 4) ? reader.ReadVector3Or4() :
                    reader.ReadVector4().AsVector3();//5.4 and up
                res.HintWeightT = reader.ReadSingle();
            }
            return res;
        }

    }

}
